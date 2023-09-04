using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // public bool mapON;
    private Transform _target;
    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            if (_target != null)
            {
                targetOffset = transform.position - _target.position;
            }
        }
    }

    Vector3 targetOffset;
    
    public void Update()
    {
        // if (Target != null && !mapON)
        // {
        //     transform.position = Vector3.Lerp(transform.position, Target.position + targetOffset, 0.1f);
        // }
    }
    
    // public void ActiveMap()
    // {
    //     mapON = true;
    // }
    
    // public void InactiveMap()
    // {
    //     mapON = false;
    // }
}