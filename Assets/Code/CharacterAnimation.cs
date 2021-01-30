using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public GameObject maskHolder;
    public float walkingSpeed = 0.5f;

    Animator anim;
    float fraction = 0f;
    bool onWalk = false;
    bool onRopeCollecting = false;

    Vector3 startPos;
    Vector3 desPos;




    private void OnEnable()
    {
        anim = GetComponent<Animator>();
    }

    public void Go(WalkDirection _dir , bool _canWalkThere, bool _onRopeCollecting)
    {
        var rotationDir = _onRopeCollecting ? 180f : 0f;
        onRopeCollecting = _onRopeCollecting;
        maskHolder.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        startPos = this.transform.position;
        switch (_dir)
        {
            case WalkDirection.none:
                break;
            case WalkDirection.up:
                anim.SetFloat("dirX", 0f);
                anim.SetFloat("dirY", 1f);

                maskHolder.transform.localRotation = Quaternion.Euler(0, 0, -90f + rotationDir);
                desPos = new Vector3(startPos.x, startPos.y, startPos.z + 1f);

                break;
            case WalkDirection.right:
                anim.SetFloat("dirX", 1f);
                anim.SetFloat("dirY", 0f);

                maskHolder.transform.localRotation = Quaternion.Euler(0, 0, -180f + rotationDir);
                desPos = new Vector3(startPos.x + 1f, startPos.y, startPos.z);

                break;
            case WalkDirection.down:
                anim.SetFloat("dirX", 0f);
                anim.SetFloat("dirY", -1f);

                maskHolder.transform.localRotation = Quaternion.Euler(0, 0, 90f + rotationDir);
                desPos = new Vector3(startPos.x, startPos.y, startPos.z - 1f);

                break;
            case WalkDirection.left:
                anim.SetFloat("dirX", -1f);
                anim.SetFloat("dirY", 0f);

                maskHolder.transform.localRotation = Quaternion.Euler(0, 0, 0f + rotationDir);
                desPos = new Vector3(startPos.x - 1f, startPos.y, startPos.z);

                break;
            default:
                break;
        }



        if (_canWalkThere)
        {
            maskHolder.transform.localScale = Vector3.one;
            maskHolder.SetActive(true);
            fraction = 0f;
            onWalk = true;
        }
    }

    private void Update()
    {
        if (onWalk)
        {
            if (fraction < 1)
            {
                fraction += Time.deltaTime * walkingSpeed;
                transform.position = Vector3.Lerp(startPos, desPos, fraction);
                if (onRopeCollecting && fraction >= 0.5)
                {
                    //TODO Delete temp rope on current tile
                }


                    if (!onRopeCollecting && fraction >= 0.35)
                {
                    maskHolder.transform.localScale = new Vector3(0.5f, 1f, 1f);
                }

            }
            else
            {
                onWalk = false;
                anim.SetFloat("dirX", 0f);
                anim.SetFloat("dirY", 0f);
                maskHolder.SetActive(false);
                maskHolder.transform.localScale = Vector3.one;
                fraction = 0f;
                onRopeCollecting = false;
            }
            
        }
    }



    [ContextMenu("Go Up")]
    public void TestGoUp()
    {
        Go(WalkDirection.up, true, false);
    }

    [ContextMenu("Go Left")]
    public void TestGoLeft()
    {
        Go(WalkDirection.left, true, false);
    }
    [ContextMenu("Go Right")]
    public void TestGoRight()
    {
        Go(WalkDirection.right, true, false);
    }
    [ContextMenu("Go Down")]
    public void TestGoDown()
    {
        Go(WalkDirection.down, true, false);
    }

    [ContextMenu("Go Up Collectiong")]
    public void TestGoUpCollectiong()
    {
        Go(WalkDirection.up,true, true);
    }

}
