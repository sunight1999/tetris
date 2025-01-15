using UnityEngine;

public class UI : MonoBehaviour
{
    public bool isPopUp = false;
    
    public virtual void SetVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}
