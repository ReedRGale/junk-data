using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject categoryManager;
    
    // Instantiate Managers if they don't exist already.
    void Awake()
    {
        if (CategoryManager.instance == null)
            Instantiate(categoryManager);
    }
}
