using Core;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeSdk : MonoConstruct
{
    [SerializeField] private SceneField _sceneField;
    
    private void Start() => Context.Resolve<SdkService>().Initialize(InitPlayerDataService);

    private void InitPlayerDataService() => Context.Resolve<PlayerDataService>().Initialize(LoadScene);
    
    private void LoadScene() => SceneManager.LoadScene(_sceneField);
}