using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetManager : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
	
	}

    public static GameObject get(string id)
    {
        return Resources.Load(id) as GameObject;
    }

    public static Sprite getSprite(string name)
    {
        return Resources.Load<Sprite>("sprites/" + name) as Sprite;
    }

    public static GameObject getParticle(string name)
    {
        return Resources.Load<GameObject>("particles/" + name);
    }
}
