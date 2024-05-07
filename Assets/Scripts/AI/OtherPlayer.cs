using UnityEngine;

namespace AIs
{
    public class OtherPlayer : MonoBehaviour
    {
        [SerializeField] private Rect playerCamRectA;
        [SerializeField] private Rect playerCamRectB;
        [SerializeField] private GameObject otherPlayerMachine;
        [SerializeField] private Camera[] cameras;
        [SerializeField] private GameObject[] virtualCamParents;
        [SerializeField] private ArcadeGame arcadeMachine;
        private IAIActions aiActions;

        private Rect resetRect = new Rect(0, 0, 1, 1);
 
        
        // Start is called before the first frame update
        public void Awake()
        {

            AI.Instance.isAIMatch = true;
            AI.Instance.AILevel = AI.AILevels.Simple;
            if (AI.Instance.isAIMatch) // isAi
            {
               aiActions = otherPlayerMachine.GetComponent<IAIActions>();
               SetAIPlayer();
            }
        }
        #region private functions

        private void SetAIPlayer()
        {
            SetCamerasForAI();
            SetSecondPlayer();
            DoInputs();
        }

        private void SetCamerasForAI()
        {
            cameras[0].rect = playerCamRectA;
            cameras[1].rect = playerCamRectB;
            cameras[1].enabled = true;
            virtualCamParents[1].SetActive(true);
        }

        private void SetSecondPlayer()
        {
            otherPlayerMachine.SetActive(true);
            arcadeMachine.IsOtherPlayer = true;
        }

        private void DoInputs()
        {
            aiActions.DoInput();
        }

        #endregion
    }
}
