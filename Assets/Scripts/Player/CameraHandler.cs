using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public static CameraHandler instance {  get; private set; }
    public static Camera mainCamera { get; private set; }
    Vector3
        position,
        rotation,
        walkingAnimVector,
        debugDriftPositionTarget,
        debugDriftPositionOutput,
        debugDriftRotationTarget,
        debugDriftRotationOutput;
    float
        debugDriftPositionSpeed,
        debugDriftRotationSpeed;

    [SerializeField] float cameraHeight = 0.825f;

    // walking anim
    [Header("Walking Animation")] 
    public bool walkingAnimEnable = true;
    [SerializeField] float
        walkingAnimSpeedMultiplier = 0.5f,
        walkingAnimVectorExponent = 1f,
        walkingAnimSpeedExponent = 1f,
        walkingAnimDelay = 0.1f;
    [SerializeField] Vector2
        walkingAnimGraph,
        walkingAnimGraphMultiplier = new Vector2(0.1f, 0.1f);
    [SerializeField] Vector3
        walkingAnimRotMultiplier = Vector3.one;
    float 
        walkingAnimDelayValue,
        walkingAnimSpeed;
    bool walkingAnimLoop;

    // position
    //[Header("Position")]
    //[SerializeField] bool positionEnable = true;
    //[SerializeField] Vector3
    //    positionScale = new(0.01f, 0.01f, 0.01f),
    //    positionMultiplier = Vector3.one;
    //[SerializeField] float
    //    positionToIdleSpeed = 15f,
    //    positionToWalkingSpeed = 5f;

    // rotation 
    [Header("Rotation")]
    [SerializeField] bool rotationEnable = true;
    //[SerializeField] Vector3 
    //    rotationTargetLeft = new(0f, 0f, 5f),
    //    rotationTargetRight = new(0f, 0f, -5f);
    [SerializeField] float
        rotationTargetLerpSpeed = 10f,
        rotationToIdleSpeed = 15f,
        rotationToWalkingSpeed = 5f;

    void Awake()
    {
        instance = this;
        mainCamera = GetComponent<Camera>();
    }
    void Update()
    {
        gameObject.transform.parent.localPosition = new Vector3(0f, cameraHeight, 0f);
        CameraAnimation();
    }
    public void CameraAnimation()
    {
        position = Player.instance.movementDirection;
        float
            positionToStateSpeed,
            rotationToStateSpeed;
        Vector3
            positionOutput = Vector3.zero;



        // check if moving
        if (position != Vector3.zero)
        {
            //positionToStateSpeed = positionToWalkingSpeed;
            rotationToStateSpeed = rotationToWalkingSpeed;
            walkingAnim();
        }
        else
        {
            position = Vector3.zero;
            rotation = Vector3.zero;
            //positionToStateSpeed = positionToIdleSpeed;
            rotationToStateSpeed = rotationToIdleSpeed;
            walkingAnimGraph.x = 0f;
            walkingAnimGraph.y = 0f;
            walkingAnimVector = Vector3.zero;
            walkingAnimDelayValue = 0f;
        }
        // all position change
        //if (positionEnable)
        //{
        //    Vector3 positionCumulative = Vector3.Scale(position, positionScale) + cameraIdleSway + walkingAnimVector;
        //    positionOutput = Vector3.Lerp(transform.localPosition, positionCumulative, Time.deltaTime * positionToStateSpeed);
        //    positionOutput.Scale(positionMultiplier);
        //    transform.localPosition = positionOutput;
        //}
        // all rotation change
        if (rotationEnable)
        {
            Quaternion rotationOutput = 
                Quaternion.Lerp(
                    transform.parent.localRotation,
                    Quaternion.Euler(
                        new Vector3(
                            MathF.Abs(walkingAnimVector.x) * walkingAnimRotMultiplier.x,
                            0f,
                            walkingAnimVector.x * walkingAnimRotMultiplier.z)),
                    Time.deltaTime * rotationToStateSpeed);
            transform.parent.localRotation = rotationOutput;

            //transform.parent.localEulerAngles = new Vector3(
            //    Mathf.Lerp(transform.parent.localEulerAngles.x, Math.Abs(walkingAnimGraph.x), Time.deltaTime * rotationToStateSpeed) * walkingAnimRotMultiplier.x,
            //    0f,
            //    Mathf.Lerp(transform.parent.localEulerAngles.z, walkingAnimGraph.x, Time.deltaTime * rotationToStateSpeed) * walkingAnimRotMultiplier.z);
            //    walkingAnimGraph.x * walkingAnimRotMultiplier.z);
        }
        #region debug data gathering
        //debugDriftPositionTarget = positionTarget;
        debugDriftPositionOutput = transform.localPosition;
        //debugDriftRotationTarget = rotationTarget;
        debugDriftRotationOutput = transform.parent.localEulerAngles;
        //debugDriftPositionSpeed = positionToStateSpeed;
        debugDriftRotationSpeed = rotationToStateSpeed;
        #endregion
    }
    void walkingAnim()
    {
        if (!walkingAnimEnable) { return; }
        if (walkingAnimDelay > 0f && walkingAnimDelayValue < walkingAnimDelay) { walkingAnimDelayValue += Time.deltaTime; return; }

        float walkingAnimValue = 0;
        //float velocityZAbs = MathF.Abs(Player.instance.fakeVelocity.z);
        float velocityZAbs = MathF.Abs(Player.instance.cameraTransformReadOnly.InverseTransformDirection(Player.instance.rb.velocity).z);
        float velocityExponentialSpeed = velocityZAbs * walkingAnimSpeedExponent;
        walkingAnimSpeed = Time.deltaTime * (velocityExponentialSpeed == 0 ? 1 : velocityExponentialSpeed) * walkingAnimSpeedMultiplier;
        if (!walkingAnimLoop)
        {
            if (walkingAnimGraph.x >= 1f) { walkingAnimLoop = true; }
            else { walkingAnimValue = walkingAnimSpeed; }
        }
        else
        {
            if (walkingAnimGraph.x <= -1f) { walkingAnimLoop = false; }
            else { walkingAnimValue = -walkingAnimSpeed; }
        }
        walkingAnimGraph.x += walkingAnimValue;
        walkingAnimGraph.y = MathF.Sin(-0.5f * MathF.PI * MathF.Pow(walkingAnimGraph.x, 2));
        float velocityExponentialRotation = velocityZAbs * walkingAnimVectorExponent;
        walkingAnimVector = walkingAnimGraph * walkingAnimGraphMultiplier * (velocityExponentialRotation == 0 ? 1 : velocityExponentialRotation);
    }

    // y + 0.1 = 0.1^x

    public StringBuilder debugGetStats()
    {
        return new StringBuilder()
            .Append("<u>Camera;</u>")
            //.Append("\nposition;").Append(" Speed: ").Append(debugDriftPositionSpeed)
            //.Append("\n Target: ").Append(debugDriftPositionTarget.Round(4).ToStringBuilder())
            //.Append("\n Output: ").Append(debugDriftPositionOutput.Round(4).ToStringBuilder())
            .Append("\nrotationOutput;").Append(debugDriftRotationOutput.Round(4).ToStringBuilder())
            .Append("\nrotationToStateSpeed: ").Append(debugDriftRotationSpeed)
            .Append("\nfov = ").Append(mainCamera.fieldOfView.FOVVerticalToHorizontal1(mainCamera))
            .Append("\nwalkingAnimSpeed = ").Append(MathF.Abs(Player.instance.rb.velocity.z)).Append(" * deltaTime ").Append(walkingAnimSpeedMultiplier).Append(" = ").Append(walkingAnimSpeed)
            .Append("\nwalkingAnimVector = ").Append(walkingAnimVector.ToStringBuilder());
    }
}