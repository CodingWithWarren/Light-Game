using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;

public class WallClingSigns : MonoBehaviour
{
    public PlayerMovement player;
    public Sprite blankSignSprite;
    private List<Transform> signs = new List<Transform>();

    private bool activated;
    private int stage = 0;

    void Awake()
    {
        signs = transform.GetComponentsInChildren<Transform>().ToList<Transform>();
        signs.Remove(signs[0]);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (player.transform.position.y >= signs[2].transform.position.y)
        {
            stage = 2;
        } else if (player.state == PlayerMovement.PlayerStates.WallClinging)
        { 
            stage = 1;
        } else if (player.state == PlayerMovement.PlayerStates.Grounded)
        {
            stage = 0;
        }

        DeactivateEverySign();

        signs[stage].GetComponent<Animator>().enabled = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        DeactivateEverySign();
    }

    private void DeactivateEverySign()
    {
        foreach (Transform sign in signs)
        {
            if (!sign.gameObject.name.Equals("Wall Cling Signs"))
            {
                sign.GetComponent<Animator>().enabled = false;
                sign.GetComponent<SpriteRenderer>().sprite = blankSignSprite;
            }
        }
    }
}
