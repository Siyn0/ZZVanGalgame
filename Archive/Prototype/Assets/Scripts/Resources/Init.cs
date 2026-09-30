using UnityEngine;
using ZZVan.Galgame;
public class Init : MonoBehaviour
{
    // Returning to the title must not clear the run or global collection.
    private void Start() { var session = GameSession.Instance; }
}
