using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drillBitScript : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D other)
    {
        RubyController controller = other.GetComponent<RubyController>();

        if (controller != null)
        {
            if (controller.health < controller.maxHealth)
            {
                controller.destructionUpdate(5);
                Destroy(gameObject);
            }
        }

    }
}