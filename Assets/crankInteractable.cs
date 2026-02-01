using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crankInteractable : MonoBehaviour
{
    public GameObject crankObject;

    public GameObject enableAfter;

    public GameObject crankEnd;
    public GameObject crankMatchEnd;

    Vector2 crankForwardDirection;
    Vector2 crankBackwardsDirection;

    public Vector2 crankForwardOffsetDirection;
    public Vector2 crankBackwardsOffsetDirection;

    playerController foundController;

    Animator playerCrankAnimator;
    GameObject playerObject;
    GameObject playerCrank;

    public GameObject doorReference;

    public float playbackTime;

    bool crankActivated;

    public GameObject interactPrompt;
    public GameObject turnPrompt;

    Vector3 crankAngle;
    // Start is called before the first frame update
    void Start()
    {
        crankAngle = new Vector3(0, 90, 0);
        foundController = FindFirstObjectByType<playerController>();
        playerCrankAnimator = foundController.playerCrankAnimator;
        playerObject = foundController.playerObject;
        playerCrank = foundController.playerCrank;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.GetComponent<playerController>())
            return;
        if (Input.GetMouseButton(0))
        {
            crankActivated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetComponent<playerController>())
            return;
        interactPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.gameObject.GetComponent<playerController>())
            return;
        interactPrompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!crankActivated)
        {
            return;
        }
        interactPrompt.SetActive(false);
        turnPrompt.SetActive(true);
        playerObject.transform.position += (transform.position - playerCrank.transform.position);
        playbackTime = crankAngle.x / 360;
        playerCrankAnimator.Play("SpinCrank", 0, playbackTime);
        playerCrankAnimator.StopPlayback();

        playerObject.transform.Rotate(0, (transform.eulerAngles.y) - playerObject.transform.eulerAngles.y, 0);
        playerObject.GetComponent<playerController>().cameraObject.transform.localEulerAngles = new Vector3(0, 0, 0);
        playerObject.GetComponent<playerController>().spinningCrank = true;

        updateCrankAngle(1);
        crankMatchEnd.transform.position = crankEnd.transform.position;
        crankForwardDirection = new Vector2(crankMatchEnd.transform.localPosition.x, crankMatchEnd.transform.localPosition.y);
        updateCrankAngle(-2);
        crankMatchEnd.transform.position = crankEnd.transform.position;
        crankBackwardsDirection = new Vector2(crankMatchEnd.transform.localPosition.x, crankMatchEnd.transform.localPosition.y);
        updateCrankAngle(1);

        crankForwardOffsetDirection = Vector3.Normalize(crankForwardDirection - crankBackwardsDirection);
        crankBackwardsOffsetDirection = Vector3.Normalize(crankBackwardsDirection - crankForwardDirection);

        Vector2 mouseInput = Vector3.Normalize(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));

        if (Vector2.Distance(mouseInput, crankForwardOffsetDirection) < 0.8)
        {
            updateCrankAngle(2f);
        }
        if (Vector2.Distance(mouseInput, crankBackwardsOffsetDirection) < 0.8)
        {
            updateCrankAngle(-2f);
        }

        crankObject.transform.localEulerAngles = playerCrank.transform.localEulerAngles;

        if (crankObject.transform.localEulerAngles.x < 0)
            crankObject.transform.Rotate(360, 0, 0);

        if (playbackTime > 1)
        {
            interactPrompt.SetActive(false);
            turnPrompt.SetActive(false);
            Destroy(this);
            Destroy(crankObject);
            playerObject.GetComponent<playerController>().spinningCrank = false;

            if (enableAfter != null)
            {
                enableAfter.SetActive(true);
            }
        }

    }

    void updateCrankAngle(float inputX)
    {
        float lastCrankAngle = crankAngle.x;
        crankAngle.x = Mathf.Max(crankAngle.x, 0);
        crankAngle.x += inputX;
        doorReference.transform.position += Vector3.up * (crankAngle.x - lastCrankAngle) / 80;
        crankObject.transform.localEulerAngles = crankAngle;
    }
}
