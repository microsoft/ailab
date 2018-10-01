// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace SnipInsight.Ink
{
    public class AcetateLayerInkCanvas : InkCanvas
    {
        List<Stroke> undoStrokes;
        List<Stroke> redoStrokes;
        List<InkCanvasEditingMode> undoActions;
        List<InkCanvasEditingMode> redoActions;

        public AcetateLayerInkCanvas()
        {
			// TODO: Fix Ink Cursor
			//using (Stream inkCursor = new MemoryStream(Properties.Resources.InkCursor))
			//{
			//	Cursor = new Cursor(inkCursor);
			//}

			Cursor = Cursors.Pen;
			EditingMode = InkCanvasEditingMode.None;
            EditingModeInverted = InkCanvasEditingMode.None;
            undoStrokes = new List<Stroke>();
            redoStrokes = new List<Stroke>();
            undoActions = new List<InkCanvasEditingMode>();
            redoActions = new List<InkCanvasEditingMode>();
        }

        internal void Clear()
        {
            Strokes.Clear();
            redoActions.Clear();
            redoStrokes.Clear();
            undoActions.Clear();
            undoStrokes.Clear();
        }

        internal void EraseAll()
        {
            Strokes.Clear();
            undoActions.Add(InkCanvasEditingMode.None);
            undoStrokes.Add(null);
            redoActions.Clear();
            redoStrokes.Clear();
        }

        internal bool HasInk()
        {
            return Strokes.Count > 0;
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            base.OnStrokeCollected(e);
            undoActions.Add(ActiveEditingMode);
            undoStrokes.Add(e.Stroke);
            redoActions.Clear();
            redoStrokes.Clear();
        }

        protected override void OnStrokeErasing(InkCanvasStrokeErasingEventArgs e)
        {
            base.OnStrokeErasing(e);
            undoActions.Add(ActiveEditingMode);
            undoStrokes.Add(e.Stroke);
            redoActions.Clear();
            redoStrokes.Clear();
        }

        protected override void OnActiveEditingModeChanged(RoutedEventArgs e)
        {
            base.OnActiveEditingModeChanged(e);

            if (ActiveEditingMode == InkCanvasEditingMode.Ink)
            {
                UseCustomCursor = true;
            }
            else
            {
                UseCustomCursor = false;
            }
        }

        /// <summary>
        /// Undo edit actions performed
        /// </summary>
        internal void Undo ()
        {
            int index = undoActions.Count - 1;
            if (index < 0)
                return;
            if (undoActions[index] == InkCanvasEditingMode.Ink)
                Strokes.RemoveAt(Strokes.Count - 1);
            else if (undoActions[index] == InkCanvasEditingMode.EraseByStroke)
                Strokes.Add(undoStrokes[index]);
            else if (undoActions[index] == InkCanvasEditingMode.None)
                for (int i = 0 ; i <index ; i++)
                {
                    if (undoActions[i] == InkCanvasEditingMode.Ink)
                        Strokes.Add(undoStrokes[i]);
                    else if (undoActions[i] == InkCanvasEditingMode.EraseByStroke)
                        Strokes.Remove(undoStrokes[i]);
                    else if (undoActions[i] == InkCanvasEditingMode.None)
                        Strokes.Clear();
                }
            redoActions.Add(undoActions[index]);
            redoStrokes.Add(undoStrokes[index]);
            undoActions.RemoveAt(index);
            undoStrokes.RemoveAt(index);
        }

        /// <summary>
        /// Redo image edits that were undone
        /// </summary>
        internal void Redo()
        {
            int index = redoActions.Count - 1;
            if (index < 0)
                return;
            if (redoActions[index] == InkCanvasEditingMode.Ink)
                Strokes.Add(redoStrokes[index]);
            else if (redoActions[index] == InkCanvasEditingMode.EraseByStroke)
                Strokes.RemoveAt(Strokes.Count - 1);
            else if (redoActions[index] == InkCanvasEditingMode.None)
                Strokes.Clear();
            undoActions.Add(redoActions[index]);
            undoStrokes.Add(redoStrokes[index]);
            redoActions.RemoveAt(index);
            redoStrokes.RemoveAt(index);
        }
    }
}
