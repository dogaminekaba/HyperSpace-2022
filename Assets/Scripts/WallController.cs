using UnityEngine;

public class WallController : MonoBehaviour
{
    static public float speed;
    static public float maxSpeed;
    private Transform t;

    // Use this for initialization
    void Start()
    {
        t = GetComponent<Transform>();
    }

    void Update()
    {
        t.Translate(0, 0, -speed * Time.deltaTime);
    }

}
