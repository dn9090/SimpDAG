using System;
using System.Collections.Generic;
using System.Numerics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SimpDAG
{
	public static class Camera
	{
		public static float dragSpeed = 10f;

		internal static Vector2f dragOrigin;

		internal static bool isDragging;

		public static void Update(RenderWindow window, float deltaTime)
		{
			if(isDragging && !Mouse.IsButtonPressed(Mouse.Button.Right))
				isDragging = false;

			if(window.HasFocus() && Mouse.IsButtonPressed(Mouse.Button.Right))
			{
				var mousePosition = Mouse.GetPosition(window);

				if(isDragging)
				{	
					var view = window.GetView();

					var dragPosition = window.MapPixelToCoords(mousePosition);
					var delta = dragPosition - dragOrigin;
					var center = view.Center + (delta * dragSpeed * deltaTime * -1f);

					view.Center = new Vector2f(center.X, center.Y);
					window.SetView(view);
				} else {
					dragOrigin = window.MapPixelToCoords(mousePosition);
					isDragging = true;
				}
			}
		}
	}
}