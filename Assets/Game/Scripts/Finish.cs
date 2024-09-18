using UnityEngine;

namespace MultiTool
{
    public class Finish : MonoBehaviour
    {

        private void OnEnable()
        {
            
        }

        private void OnDisable()
        {
            
        }

        private void Awake()
        {
            
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.TryGetComponent(out PlayerController player))
            {
                player.Stop();
                GameManager.Instance.Finish();
                Destroy(this);
            }
        }
    }

}