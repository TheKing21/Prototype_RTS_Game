using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IEnvironment : MonoBehaviour {

    protected int _woodAmount = 0;
    protected int _stoneAmount = 0;
    protected int _goldAmount = 0;
    protected int _foodAmount = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeRessource(int Amount) { }
}
