using UnityEngine;
using DarkRift.Client.Unity;

public class Player : MonoBehaviour
{
    public UnityClient Client { get; set; }
    public int ID { get; set; }
    public bool IsControllable { get; set; }
    public bool IsServer { get; set; }
    public string Nickname { get; set; }
    public byte Skin { get; set; }
    



}
