using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace WonderGather
{
    public sealed class FactionCreator : MonoBehaviour
    {
        [SerializeField] private CivilizationDefinition example;
        [SerializeField] private Camera creatorCamera;
        [SerializeField] private string playtestScene="FactionPlaytest";
        private Scene map;
        public FactionDraft Draft {get;private set;}
        public bool Playing {get;private set;}
        public bool Busy {get;private set;}
        public string Status {get;private set;}="Choose a blueprint card to shape its relationships.";
        public FactionPlaytestBridge Bridge {get;private set;}
        public Rect ReturnButton=>new Rect(Screen.width-222,18,204,42);
        public void Configure(CivilizationDefinition source,Camera camera){example=source;creatorCamera=camera;}
        private void Awake()
        {
            if(example==null){Status="Creator example is missing.";enabled=false;return;}
            Draft=new FactionDraft(example);
        }
        public bool Playtest()
        {
            if(Busy || Playing || Draft==null) return false;
            Draft.Refresh();if(!Draft.Report.CanInstantiate){Status="Resolve the setup messages before playtesting.";return false;}
            Busy=true;Status="Opening your faction's test map...";StartCoroutine(OpenMap());return true;
        }
        private IEnumerator OpenMap()
        {
            yield return SceneManager.LoadSceneAsync(playtestScene,LoadSceneMode.Additive);
            map=SceneManager.GetSceneByName(playtestScene);
            // Inspect only the explicitly loaded scene, once, for its authored bridge.
            foreach(var root in map.GetRootGameObjects())
            {Bridge=root.GetComponentInChildren<FactionPlaytestBridge>();if(Bridge!=null) break;}
            if(Bridge==null){Status="The playtest map is missing its setup bridge.";yield return SceneManager.UnloadSceneAsync(map);Busy=false;yield break;}
            SceneManager.SetActiveScene(map);
            if(!Bridge.Begin(Draft.Definition,BlocksPointer))
            {
                Status=Bridge.Session.Summary;SceneManager.SetActiveScene(gameObject.scene);
                yield return SceneManager.UnloadSceneAsync(map);Bridge=null;Busy=false;yield break;
            }
            creatorCamera.enabled=false;Playing=true;Busy=false;
        }
        public bool ReturnToCreator()
        {
            if(Busy || !Playing) return false;
            Busy=true;StartCoroutine(CloseMap());return true;
        }
        private IEnumerator CloseMap()
        {
            SceneManager.SetActiveScene(gameObject.scene);
            yield return SceneManager.UnloadSceneAsync(map);
            Bridge=null;creatorCamera.enabled=true;Playing=false;Busy=false;
            Status="Back in your draft. Your blueprint choices are preserved; the next playtest starts fresh.";
        }
        private bool BlocksPointer(Vector2 position)=>ReturnButton.Contains(new Vector2(position.x,Screen.height-position.y));
        private void OnDestroy(){Draft?.Dispose();}
    }
}
