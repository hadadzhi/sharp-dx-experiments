using System.Collections.Generic;
using System.Windows.Forms;
using SharpDXCommons;
using SolarSystemDemo.MineCraft.Structures.Ships;
using SolarSystemDemo.Objects.Base;

namespace SolarSystemDemo.Objects.Demo.PlayerControls
{
	public delegate void OnKeyDownOperation();
	public delegate void OnKeyUpOperation();
	public delegate void OnMouseWheelOperation(object sender, MouseEventArgs e);

	public class ShipControls
	{
		private Dictionary<Keys, List<IActivatable>> EngineBinds;
		private Dictionary<Keys, OnKeyDownOperation> OnKeyDownOperations;
		private Dictionary<Keys, OnKeyUpOperation> OnKeyUpOperations;
		private OnMouseWheelOperation OnMouseWheelOperations;


		public ShipControls()
		{
			EngineBinds = new Dictionary<Keys, List<IActivatable>>();
			OnKeyDownOperations = new Dictionary<Keys, OnKeyDownOperation>();
			OnKeyUpOperations = new Dictionary<Keys, OnKeyUpOperation>();
		}


		public void InstallControls(RenderWindow renderWindow)
		{
			renderWindow.MouseWheel += OnMouseWheelEventHandler;
			renderWindow.KeyDown += KeyDownEventHandler;
			renderWindow.KeyUp += KeyUpEventHandler;
		}

		public void RemoveControls(RenderWindow renderWindow)
		{
			renderWindow.MouseWheel -= OnMouseWheelEventHandler;
			renderWindow.KeyDown -= KeyDownEventHandler;
			renderWindow.KeyUp -= KeyUpEventHandler;
		}

		public void Bind(Keys key, IActivatable activeBlockRef)
		{
			if (EngineBinds.ContainsKey(key))
			{
				if (EngineBinds[key].Contains(activeBlockRef))
				{
					return;
				}
				else
				{
					EngineBinds[key].Add(activeBlockRef);
				}
			}
			else
			{
				EngineBinds.Add(key, new List<IActivatable>() { activeBlockRef });
			}
		}

		public void BindOnKeyDownOperation(Keys key, OnKeyDownOperation operation)
		{
			if (OnKeyDownOperations.ContainsKey(key))
			{
				OnKeyDownOperations[key] += operation;
			}
			else
			{
				OnKeyDownOperations.Add(key, operation);
			}
		}

		public void BindOnKeyUpOperation(Keys key, OnKeyUpOperation operation)
		{
			if (OnKeyUpOperations.ContainsKey(key))
			{
				OnKeyUpOperations[key] += operation;
			}
			else
			{
				OnKeyUpOperations.Add(key, operation);
			}
		}

		public void BindOnMouseWheelOperation(OnMouseWheelOperation operation)
		{
			OnMouseWheelOperations += operation;
		}

		#region Event Handlers

		private void KeyDownEventHandler(object sender, KeyEventArgs e)
		{
			Keys key = e.KeyCode;

			if (EngineBinds.ContainsKey(key))
			{
				foreach (IActivatable engine in EngineBinds[key])
				{
					engine.Activate();
				}
			}

			if (OnKeyDownOperations.ContainsKey(key))
			{
				OnKeyDownOperations[key]();
			}
		}

		private void KeyUpEventHandler(object sender, KeyEventArgs e)
		{
			Keys key = e.KeyCode;

			if (EngineBinds.ContainsKey(key))
			{
				foreach (IActivatable engine in EngineBinds[key])
				{
					engine.Deactivate();
				}
			}

			if (OnKeyUpOperations.ContainsKey(key))
			{
				OnKeyUpOperations[key]();
			}
		}

		private void OnMouseWheelEventHandler(object sender, MouseEventArgs e)
		{
			if (OnMouseWheelOperations != null)
			{
				OnMouseWheelOperations(sender, e);
			}
		}

		#endregion Event Handlers

		//public static OnMouseWheelOperation CreateInertiaEnginesControlEventHandler(
		//    List<InertiaEngine> towardEngines,
		//    List<InertiaEngine> backwardEngines)
		//{
		//    // есть уязвимость к ошибкам при контроле этих двигателей разными способами
		//    return (object sender, MouseEventArgs e) =>
		//    {
		//        float delta = (float) (e.Delta / Math.Abs(e.Delta)) / 5;

		//        float towardLimit = 0;
		//        float backwardLimit = 0;

		//        if (towardEngines != null && towardEngines.Count != 0)
		//        {
		//            towardLimit = towardEngines[0].PercentageLimit;
		//        }

		//        if (backwardEngines != null && backwardEngines.Count != 0)
		//        {
		//            backwardLimit = backwardEngines[0].PercentageLimit;
		//        }

		//        float p = towardLimit - backwardLimit + delta;
		//        p = TwaMath.Clamp(p, -1, 1);

		//        if (p > 0)
		//        {
		//            if (towardEngines != null)
		//            {
		//                foreach (InertiaEngine engine in towardEngines)
		//                {
		//                    engine.PercentageLimit = p;
		//                    engine.Activate();
		//                }
		//            }

		//            if (backwardEngines != null)
		//            {
		//                foreach (InertiaEngine engine in backwardEngines)
		//                {
		//                    engine.PercentageLimit = 0;
		//                    engine.Deactivate();
		//                }
		//            }
		//        }
		//        else
		//        {
		//            if (towardEngines != null)
		//            {
		//                foreach (InertiaEngine engine in towardEngines)
		//                {
		//                    engine.PercentageLimit = 0;
		//                    engine.Deactivate();
		//                }
		//            }

		//            if (backwardEngines != null)
		//            {
		//                foreach (InertiaEngine engine in backwardEngines)
		//                {
		//                    engine.PercentageLimit = -p;
		//                    engine.Activate();
		//                }
		//            }
		//        }
		//    };
		//}
	}
}
