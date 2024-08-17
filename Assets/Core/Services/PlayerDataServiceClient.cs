using Lib;

namespace Core.Services
{
    public class PlayerDataServiceClient : MonoConstruct
    {
        private PlayerDataService _service;
        
        private void Awake() => _service = Context.Resolve<PlayerDataService>();

        public void DeletePlayerData() => _service.Reset();
    }
}