namespace Woodblight;

using Chickensoft.LogicBlocks;
using Godot;

public partial class EnemyLogic {
  public partial record State {
    public partial record Alive {
      public partial record Aggroed : Alive, IGet<Input.Move>, IGet<Input.Age> {
        public Aggroed() {
          OnAttach(() => {
            var gameRepo = Get<IGameRepo>();
            gameRepo.PlayerGlobalPosition.Sync += OnPlayerGlobalPositionSync;
          });
          OnDetach(() => {
            var gameRepo = Get<IGameRepo>();
            gameRepo.PlayerGlobalPosition.Sync -= OnPlayerGlobalPositionSync;
          });

          this.OnEnter(() => Output(new Output.AnimationUpdated("crawl")));
        }

        private void OnPlayerGlobalPositionSync(Vector2 vector) =>
          Output(new Output.PlayerTrackedAt(vector));

        public Transition On(in Input.Move input) {
          if (input.Direction.X > 0) {
            Output(new Output.FlipSprite(true));
          }
          else if (input.Direction.X < 0) {
            Output(new Output.FlipSprite(false));
          }
          Output(new Output.ForceApplied(input.Direction * Get<Data>().Speed, false));
          return ToSelf();
        }

        public Transition On(in Input.Age input) {
          var roomRepo = Get<IRoomRepo>();
          var data = Get<Data>();
          var settings = Get<IEnemySettings>();

          data.TimeAggroed += input.Time;

          if (settings.Breeds && data.TimeAggroed > 2) {
            if (roomRepo.EnemyCount.Value < 100) {
              Output(new Output.SpawnLarva());
              Get<IRoomRepo>().OnEnemySpawned();
            }
            data.TimeAggroed = 0;
          }

          return settings.Metamorphizes
            ? data.TimeAggroed < 5
                           ? ToSelf()
                           : To<Metamorphosis>()
            : ToSelf();
        }
      }
    }
  }
}
