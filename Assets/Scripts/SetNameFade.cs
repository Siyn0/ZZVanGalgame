using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetNameFade : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 acc = Vector3.zero;
        Vector3 diff;

        transform.localScale = new Vector3(1, 1, 1);
        acc.x = 0.2f;

        diff = Vector3.MoveTowards(transform.localPosition, transform.localPosition + acc, 0.5f * Time.time);
        transform.localPosition = diff;

    }
}
