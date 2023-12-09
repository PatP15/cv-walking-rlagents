
# CV and RL Walking Agents
Active ragdoll training with Unity ML-Agents (PyTorch). 

## Ragdoll Agent

Based on [walker example](https://github.com/Unity-Technologies/ml-agents/blob/main/docs/Learning-Environment-Examples.md)
Built off of [walker github](https://github.com/kressdev/RagdollTrainer)
The Robot Kyle model from the Unity assets store is used for the ragdoll.

![RobotKyleBlend Image](/docs/RobotKyleBlend.png)

### Features:

* Default Robot Kyle rig replaced with a new rig created in blender. FBX and blend file included.

* Heuristic function inlcuded to drive the joints by user input (for development testing only).

* Added stabilizer to hips and spine. The stabilizer applies torque to help ragdoll balance.

* Added "earlyTraining" bool for initial balance/walking toward target.

* Added WallsAgent prefab for navigating around obstacles (using Ray Perception Sensor 3D).

* Added StairsAgent prefab for navigating small and large steps.

* Added curiosity to yaml to improve walls and stairs training.

* Added two environement one for obstacle detection and another for terrain detection

* Integrate YOLOv8 with Barracuda and pipe the outputs into the observation space of the RL agent


