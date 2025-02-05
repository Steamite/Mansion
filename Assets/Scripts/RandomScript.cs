using UnityEngine;

public class RandomScript : MonoBehaviour
{
    [SerializeField] [Range(1, 5)] float numberA;
    [SerializeField] [Range(1, 5)] int numberB;
    [SerializeField] string message { get; set; }
}
