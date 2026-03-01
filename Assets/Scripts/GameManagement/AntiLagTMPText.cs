using UnityEngine;

public class AntiLagTMPText : MonoBehaviour
{
    // SVRHA OVE SKRIPTE JE INICIJALIZIRATI TMP TEXT KAKO NEBI LAGGALO
    // Na prvoj pojavi TMP_Text-a, dogodi se masovni lag spike
    public GameObject textPrefab;
    
    void Awake()
    {
        var ft = new FloatingText
        {
            obj = Instantiate(textPrefab)
        };
        ft.obj.GetComponent<MeshRenderer>().sortingLayerName = "Collision";
        ft.obj.GetComponent<MeshRenderer>().sortingOrder = 9999;
        ft.text = ft.obj.GetComponent<TextMesh>();
        ft.obj.transform.position = new Vector3(9999, 9999, 9999);
        ft.Show();
    }
}
