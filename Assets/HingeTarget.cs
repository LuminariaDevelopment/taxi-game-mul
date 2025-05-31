using UnityEngine;

public class HingeTarget : MonoBehaviour
{
    public float targetAngle = 0f; // Desired resting angle
    public float springStrength = 50f;
    public float springDamper = 5f;

    private HingeJoint hingeJoint;

    void Start()
    {
        hingeJoint = GetComponent<HingeJoint>();
        ConfigureSpring();
    }

    void Update()
    {
        // If joint is gone (e.g., part broke off), stop trying to update it
        if (hingeJoint == null)
            return;

        if (hingeJoint.connectedBody != null)
        {
            JointSpring spring = hingeJoint.spring;
            spring.targetPosition = targetAngle;
            hingeJoint.spring = spring;
        }
    }

    void ConfigureSpring()
    {
        if (hingeJoint != null)
        {
            JointSpring spring = new JointSpring
            {
                spring = springStrength,
                damper = springDamper,
                targetPosition = targetAngle
            };

            hingeJoint.spring = spring;
            hingeJoint.useSpring = true;
        }
    }
}
