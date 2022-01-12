using UnityEngine;

namespace IKVR.ExeInEditMode
{
    [ExecuteInEditMode]
    public class MirrorTRS : MonoBehaviour
    {
        private Transform _transform;
    
        private void Awake()
        {
            _transform = transform;
        }

        private void OnGUI()
        {
            var posY = _transform.position.y * 2f;
            _transform.localScale = new Vector3(posY, posY, posY);
        }
    }
}
