using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
   public enum MoveType
   {
      MoveDistance,
      MoveToTarget,
      MoveToClick
   }
   
   [Header("Movement: ")]
   [SerializeField] private MoveType moveType;
   [Space]
   [SerializeField] private float moveDistance;
   [SerializeField] private float moveTime;
   [Space]
   [SerializeField] private Transform targetPosition;

   [Header("Click Movement: ")]
   [SerializeField] private Camera mainCamera;
   
   [Header("Score: ")]
   [SerializeField] private TextMeshProUGUI scoreText;
   [SerializeField] private int score;


   void Start()
   {
      UpdateScore();
   }

   void Update()
   {
      //Space Input Movement:
      if (Input.GetKeyDown(KeyCode.Space))
      {
         Move();
      }

      //Click Input Movement:
      if (Input.GetMouseButtonDown(0))
      {
         if (moveType == MoveType.MoveToClick)
         {
            MoveToClickedPosition(Input.mousePosition);
         }
         else
         {
            Move();
         }
      }
   }

   void Move()
   {
      //Forward Movement:
      if (moveType == MoveType.MoveDistance)
      {
         Vector3 newPos = transform.position + (moveDistance * Vector3.forward);
         transform.DOMove(newPos, moveTime);
      }

      //Targeted Movement:
      if (moveType == MoveType.MoveToTarget)
      {
         transform.DOMove(targetPosition.position, moveTime);
      }
   }
   
   void MoveToClickedPosition(Vector2 screenPosition)
   {
      Ray ray = mainCamera.ScreenPointToRay(screenPosition);
      
      //Clicked Movement:
      if (Physics.Raycast(ray, out var hit))
      {
         Vector3 newPos = hit.point;
         newPos.y = transform.position.y;
         transform.DOMove(newPos, moveTime).SetEase(Ease.OutExpo);
      }
   }

   void OnTriggerEnter(Collider  other)
   {
      if (other.CompareTag("Coin"))
      {
         Destroy(other.gameObject);
         score++;
         UpdateScore();
      }
      
      if (other.CompareTag("Enemy"))
      {
         Debug.Log("GAME OVER");
      }
   }

   //
   void UpdateScore()
   {
      scoreText.text = score.ToString();
   }
   
   //Public Accessors:
   public MoveType GetMoveType() => moveType;
   public float GetMoveDistance() => moveDistance;
   public float GetMoveTime() => moveTime;
   public  Transform GetTargetPosition() => targetPosition;
   public Camera  GetMainCamera() => mainCamera;
   public TextMeshProUGUI GetScoreText() => scoreText;
   public int GetScore() => score;
   
}
