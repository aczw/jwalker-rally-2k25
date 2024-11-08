using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Given a list of points and player position, spawn a car


public class ObstacleManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject RallyCar;
    public GameObject[] Targets;
    private RallyController RC_Controller;

    /*
     * SpawnDeathCars()
     * {
     *  GameObject Death
       }
     */
    public void PositionCar(Vector3 pos) {
        if (RallyCar == null) return;
        RallyCar.transform.position = pos;
    }

    // this is for in-editor stuff
    public void InitializeFromObjects(GameObject[] targets) {
        var posTargets = new List<Vector3>();
        foreach (var target in targets) {
            posTargets.Add(target.transform.position);
        }

        InitializeRC(posTargets.ToArray());
    }

    public void InitializeRC(Vector3[] targets) {
        if (targets == null) {
            return;
        }

        //Debug.Log(targets);
        RC_Controller = new RallyController(RallyCar, targets, false);
    }


    // Update is called once per frame
    private void FixedUpdate() {
        if (RC_Controller == null) {
            return;
        }

        RC_Controller.LaunchCar();
    }
}

public class RallyController
{
    private GameObject RallyCar;
    private Rigidbody CarRB;
    private Vector3[] Targets;
    private int CurrentTargetIdx = 0;
    private float AdditionalThrust = 2000000;
    private float SteeringPower = 200000.0f;

    public RallyController(GameObject car, Vector3[] Targets, bool DynamicParams) {
        RallyCar = car;
        CarRB = RallyCar.GetComponent<Rigidbody>();
        this.Targets = Targets;

        // additional thrust 
        if (DynamicParams) {
            AdditionalThrust = Random.Range(4.0f, 8.0f);
            SteeringPower = Random.Range(4.0f, 8.0f);
        }
    }

    private float DistanceToTarget(Vector3 target) {
        return (target - RallyCar.transform.position).magnitude;
    }

    public void LaunchCar() {
        if (CurrentTargetIdx >= Targets.Length) {
            Object.Destroy(RallyCar);
            return;
        }

        if (CarRB == null) return;

        var carToBegin = Targets[CurrentTargetIdx] - CarRB.position;
        var launchDir = Vector3.Normalize(carToBegin);
        var BeginDist = carToBegin.magnitude;

        Debug.DrawRay(CarRB.position, launchDir, Color.white);

        if (BeginDist < 4f) {
            CurrentTargetIdx++;

            if (CurrentTargetIdx >= Targets.Length) {
                return;
            }

            CarRB.velocity = CarRB.velocity * 1.5f / Mathf.Sqrt(Vector3.Magnitude(CarRB.velocity));
        }

        //Vector3 forward = RallyCar.transform.forward;
        //Vector3 cross = Vector3.Cross(forward, launchDir);
        //float angleToTarget = Vector3.SignedAngle(forward, launchDir, Vector3.up);
        //angleToTarget *= Mathf.PI / 180.0f;

        //CarRB.AddTorque(cross * angleToTarget  * SteeringPower *4 * Time.fixedDeltaTime);
        //RallyCar.transform.LookAt(Targets[CurrentTargetIdx]);

        var target = Targets[CurrentTargetIdx];
        var rotation = Quaternion.LookRotation(target - RallyCar.transform.position);
        //rotation.x = 0; 
        //rotation.z = 0; 
        RallyCar.transform.rotation = Quaternion.Slerp(RallyCar.transform.rotation, rotation, Time.fixedDeltaTime * 10);
        CarRB.AddForce(RallyCar.transform.forward * (AdditionalThrust * Time.fixedDeltaTime));
        CarRB.AddForce(-Vector3.Normalize(RallyCar.transform.position - RCC.I.Player.transform.position) *
                       AdditionalThrust / 5 * Time.fixedDeltaTime);
    }
}