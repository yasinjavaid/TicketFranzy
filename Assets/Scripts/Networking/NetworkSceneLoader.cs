using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class NetworkSceneLoader : MonoBehaviour
    {
       [SerializeField]
          protected string sceneName;
          [SerializeField]
          protected LoadSceneMode loadSceneMode;
          [SerializeField]
          protected bool loadAsync = false;
          [SerializeField]
          protected bool preload = false;
      
          protected AsyncOperation asyncOperation;
          protected EasyCoroutine loadCoroutine;
      
          private IEnumerator Start()
          {
              if (loadAsync && preload)
              {
                  yield return null;
                  PhotonNetwork.LoadLevel(sceneName);
                  //asyncOperation.allowSceneActivation = false;
              }
          }
      
          public void LoadScene() => EasyCoroutine.StartNew(ref loadCoroutine, LoadScene(sceneName, loadAsync));
      
          private IEnumerator LoadScene(string sceneName, bool loadAsync = false)
          {
              if (asyncOperation != null)
              {
                  yield return WaitForCanvasFader();
                  asyncOperation.allowSceneActivation = true;
                  asyncOperation.completed += (_) => asyncOperation = null;
                  yield return asyncOperation;
              }
              else
              {
                  if (loadAsync)
                  {
                      asyncOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                      asyncOperation.allowSceneActivation = false;
                      yield return WaitForCanvasFader();
      
                      asyncOperation.allowSceneActivation = true;
                      asyncOperation.completed += (_) => asyncOperation = null;
                      yield return asyncOperation;
                  }
                  else
                  {
                      yield return WaitForCanvasFader();
                      Debug.Log("Here");
                      NetworkManager.Instance.LoadPhotonScene(sceneName);
                  }
              }
          }
      
          private IEnumerator WaitForCanvasFader()
          {
              if (SceneFader.CanvasFader)
              {
                  SceneFader.CanvasFader.FadeIn();
                  yield return new WaitUntil(() => SceneFader.CanvasFader.IsFullyVisible);
              }
          }
    }
}
