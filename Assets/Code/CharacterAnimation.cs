using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public GameObject maskHolder;
    public float walkingSpeed = 0.5f;
    public GameObject wolleObject;

    public AudioSource soundSource;
    public AudioSource walkingSoundSource;
    public AudioClip walkNo;
    public AudioClip eatingChips;
    public AudioClip noRope;
    public AudioClip collectingRope;


    Animator anim;
    float fraction = 0f;
    bool onWalk = false;
    bool onRopeCollecting = false;

    Vector3 startPos;
    Vector3 desPos;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Idle", true);
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
            anim.SetBool("Idle", false);
            walkingSoundSource.Play();
            if (onRopeCollecting)
            {
                soundSource.PlayOneShot(collectingRope);
            }

        }
        else
        {
            soundSource.PlayOneShot(walkNo);
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
                    MazeController.DeleteTempRope();
                }


                    if (!onRopeCollecting && fraction >= 0.4)
                {
                    maskHolder.transform.localScale = new Vector3(0.5f, 1f, 1f);
                }

            }
            else
            {
                walkingSoundSource.Stop();
                anim.SetBool("Idle", true);
                onWalk = false;
                maskHolder.SetActive(false);
                maskHolder.transform.localScale = Vector3.one;
                fraction = 0f;
                onRopeCollecting = false;
            }
            
        }
        if (MazeController.MaxRopeLength < 1) MazeController.MaxRopeLength = 1;
        float wolleXFraction = (float)MazeController.CurrentRopeLength / (float)MazeController.MaxRopeLength;
        wolleObject.transform.localScale = new Vector3(Mathf.Lerp(0.5f, 1f, wolleXFraction),wolleObject.transform.localScale.y, wolleObject.transform.localScale.z);
    }

    public void EatChips()
    {
        anim.SetTrigger("Eat");
        soundSource.PlayOneShot(eatingChips);
    }

    public void NoRope()
    {
        soundSource.PlayOneShot(noRope);
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
