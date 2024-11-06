# Physics Dropper Overlay for Unity

![Physics Dropper in Action](docs/images/ExampleGif.gif)

[![Join our Discord](https://img.shields.io/badge/Discord-Join%20Us-7289DA?logo=discord&logoColor=white)](https://discord.gg/knwtcq3N2a)
![Discord](https://img.shields.io/discord/1047781241010794506)

![GitHub Forks](https://img.shields.io/github/forks/Ddemon26/TCS-PhysicsDropper)
![GitHub Contributors](https://img.shields.io/github/contributors/Ddemon26/TCS-PhysicsDropper)
![GitHub Stars](https://img.shields.io/github/stars/Ddemon26/TCS-PhysicsDropper)
![GitHub Repo Size](https://img.shields.io/github/repo-size/Ddemon26/TCS-PhysicsDropper)

## Overview

The **Physics Dropper Overlay** is a specialized tool designed for Unity developers, allowing for efficient physics-based placement of GameObjects directly within Unity's SceneView. By enabling the dropping of selected objects using simulated physics, this tool streamlines the positioning process and ensures objects are placed naturally without the tedious need for manual adjustments.

Using the Physics Dropper Overlay helps add realism to your scene by leveraging Unity's physics engine to naturally position GameObjects. This makes it especially useful for creating dynamic environments, setting up test cases, or managing multiple objects within a scene, ultimately speeding up development while maintaining precision.

## Key Features

- **SceneView Integration**: Provides a seamless overlay within Unity's SceneView, allowing quick and easy access to the dropper tool.
- **Toolbar Access**: The dropper functionality is accessible via a dedicated toolbar button for convenience.
- **Configurable Stopping Criteria**: Choose when the simulation should stop based on either time elapsed after impact or a velocity threshold.
- **Automatic Component Management**: Automatically adds necessary physics components like Colliders and Rigidbodies if they are not already present on the GameObjects.
- **Support for MeshCollider Convex Settings**: Ensures that MeshColliders are compatible with dynamic physics simulations by adjusting their convex settings.
- **Max Simulation Time Setting**: Avoids endless simulations by letting users define a maximum allowable duration for the drop simulation.

## How It Works

The **Physics Dropper Overlay** simplifies the process of arranging GameObjects in Unity by enabling the use of simulated physics within the editor. The system ensures that each selected GameObject is equipped with the necessary physics components and manages the entire simulation process in real time.

The workflow involves the following steps:

1. **Select GameObjects**: Highlight the GameObjects in the SceneView that you want to drop.
2. **Activate Dropper**: Click the custom toolbar button to initiate the dropper tool. This starts the physics simulation, dropping the selected objects in the SceneView.
3. **Simulation Criteria**: The drop continues until one of the stopping criteria is satisfied:
   - **Time After Impact**: The simulation persists for a set time after the object first makes contact with another surface.
   - **Velocity Threshold**: The simulation halts when the velocity of all dropped objects falls below a predefined threshold.
4. **Automatic Cleanup**: Once the simulation is complete, the tool reverts any temporary components to return the GameObjects to their original state.

## Installation

To install the Physics Dropper Overlay:

1. Clone or download this repository.
2. Add the `PhysicsDropperOverlay.cs` script to your Unity project under the `Editor` directory.
3. Launch Unity, and the Physics Dropper Overlay will be available for use within the SceneView.

## Usage

To use the Physics Dropper Overlay:

1. **Select GameObjects**: In the SceneView, select the GameObjects that you intend to drop.
2. **Click the Toolbar Button**: Locate and click the `PhysicsDropToolbarButton` in the SceneView toolbar to open the dropper interface.
3. **Configure Drop Settings**:
   - **Stopping Criteria**: Set the criteria for when the simulation should end:
      - **Time After Impact**: Define the duration to continue simulating after the object lands.
      - **Velocity Threshold**: Specify the velocity value below which the simulation should stop.
   - **Maximum Simulation Time**: Set a limit for the total simulation duration to prevent overly long or infinite drops.
4. **Start Simulation**: Toggle the button to initiate the drop. The selected GameObjects will then fall and interact naturally based on physics simulation.

### Example Scenario

Suppose you are designing a forest environment and need to place rocks, branches, and other natural objects on uneven terrain. Instead of manually adjusting each object's position, use the Physics Dropper tool to simulate dropping them, allowing them to settle realistically on the terrain's surface.

## Toolbar Button and Interface

- **PhysicsDropToolbarButton**: This is the primary way to interact with the Physics Dropper. The button, located in Unity's SceneView toolbar, allows you to start, configure, and stop the physics simulation.
- **PhysicsDropperWindow**: A popup window that provides controls for modifying parameters such as the stopping criteria and the maximum allowable simulation time.

## Contributing

We appreciate contributions that can improve the Physics Dropper Overlay's features and usability. If you have suggestions for new features or optimizations, feel free to fork the repository, make changes, and submit a pull request.

### Local Development

1. Fork this repository.
2. Create a new branch for your feature (`git checkout -b feature/NewFeature`).
3. Commit your modifications (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature/NewFeature`).
5. Open a Pull Request for review.

## Support

If you encounter issues or have questions about the Physics Dropper Overlay, join our Discord community using the link at the top of this README.

## License

This project is licensed under the MIT License, allowing free use, modification, and distribution.

---

For more comprehensive information, refer to the comments within the codebase or create an issue in the GitHub repository. Happy coding!
