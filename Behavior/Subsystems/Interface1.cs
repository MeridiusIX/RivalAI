using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace RivalAI.Behavior.Subsystems {
	public interface IAutoPilotControl {

		void DisableAutopilot();

		void SetWaypoint(Vector3D coords);



	}
}
