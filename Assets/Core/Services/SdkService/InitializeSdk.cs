using System.Collections;
using Core;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeSdk : MonoConstruct
{
    [SerializeField] private SceneField _sceneField;
    
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(.5f);
        Context.Resolve<SdkService>().Initialize(InitPlayerDataService);
    }

    private void InitPlayerDataService() => Context.Resolve<PlayerDataService>().Initialize(LoadScene);
    
    private void LoadScene() => SceneManager.LoadScene(_sceneField);
}