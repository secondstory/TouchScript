/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using TUIOsharp;
using UnityEngine;

namespace TouchScript.InputSources
{
    /// <summary>
    /// Processes TUIO 1.0 input.
    /// </summary>
    [AddComponentMenu("TouchScript/Input Sources/TUIO Input")]
    public class TuioInput : InputSourcePro
    {
        #region Unity fields

        /// <summary>
        /// Port to listen to.
        /// </summary>
        public int TuioPort = 3333;

        /// <summary>
        /// Minimum movement delta to ignore in cm.
        /// </summary>
        public float MovementThreshold = 0f;

        #endregion

        #region Private variables

        private TuioServer server;
        private Dictionary<TuioCursor, int> cursorToInternalId = new Dictionary<TuioCursor, int>();
        private Dictionary<TuioObject, int> objectToInternalId = new Dictionary<TuioObject, int>();
        private int screenWidth;
        private int screenHeight;
        private static TuioInput instance;
        // Flag to indicate that we are going out of Play Mode in the editor. Otherwise there might be a loop when while deinitializing other objects access TouchScript.Instance which recreates an instance of TouchManager and everything breaks.
        private static bool shuttingDown = false;
        #endregion

        #region Unity

        /// <inheritdoc />
        protected override void Start()
        {
            base.Start();

            server = new TuioServer(TuioPort);
            server.MovementThreshold = MovementThreshold*TouchManager.Instance.DotsPerCentimeter/Mathf.Max(Screen.width, Screen.height);
            server.CursorAdded += OnCursorAdded;
            server.CursorUpdated += OnCursorUpdated;
            server.CursorRemoved += OnCursorRemoved;
            //server.ObjectAdded += server_ObjectAdded;
            //server.ObjectUpdated += server_ObjectUpdated;
            //server.ObjectRemoved += server_ObjectRemoved;
            server.Connect();
            
        }
        public static TuioInput Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance == null)
                {
                    instance = FindObjectOfType(typeof(TuioInput)) as TuioInput;
                    if (instance == null && Application.isPlaying)
                    {
                        var go = GameObject.Find("TouchScript");
                        if (go == null) go = new GameObject("TouchScript");
                        instance = go.AddComponent<TuioInput>();
                    }
                }
                return instance;
            }
        }
        public TuioServer Server { get { return server; } }
        /// <inheritdoc />
        protected override void Update()
        {
            base.Update();
            screenWidth = Screen.width;
            screenHeight = Screen.height;
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            if (server != null)
            {
                server.CursorAdded -= OnCursorAdded;
                server.CursorUpdated -= OnCursorUpdated;
                server.CursorRemoved -= OnCursorRemoved;
                server.Disconnect();
            }
            if (!Application.isLoadingLevel) shuttingDown = true;
            base.OnDestroy();
        }

        #endregion

        #region Event handlers

        private void OnCursorAdded(object sender, TuioCursorEventArgs tuioCursorEventArgs)
        {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this)
            {
                var x = cursor.X*screenWidth;
                var y = (1 - cursor.Y)*screenHeight;
                cursorToInternalId.Add(cursor, beginTouch(new Vector2(x, y)));
            }
        }

        private void OnCursorUpdated(object sender, TuioCursorEventArgs tuioCursorEventArgs)
        {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this)
            {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                var x = cursor.X*screenWidth;
                var y = (1 - cursor.Y)*screenHeight;

                moveTouch(existingCursor, new Vector2(x, y));
            }
        }

        private void OnCursorRemoved(object sender, TuioCursorEventArgs tuioCursorEventArgs)
        {
            var cursor = tuioCursorEventArgs.Cursor;
            lock (this)
            {
                int existingCursor;
                if (!cursorToInternalId.TryGetValue(cursor, out existingCursor)) return;

                cursorToInternalId.Remove(cursor);
                endTouch(existingCursor);
            }
        }

        //private void server_ObjectAdded(object sender, TuioObjectEventArgs e)
        //{
        //    var obj = e.Object;
        //    lock (this)
        //    {
        //        var x = obj.getScreenX( screenWidth);
        //        var y = obj.getScreenY( screenHeight);
        //        objectToInternalId.Add(obj, beginObject(new Vector2(x, y)));
        //    }
        //}

        //private void server_ObjectUpdated(object sender, TuioObjectEventArgs e)
        //{
        //    var obj = e.Object;
        //    lock (this)
        //    {
        //        int existingObject;
        //        if (!objectToInternalId.TryGetValue(obj, out existingObject)) return;

        //        var x = obj.getScreenX(screenWidth);
        //        var y = obj.getScreenY(screenHeight);

        //        moveObject(existingObject, new Vector2(x, y));
        //    }
        //}

        //private void server_ObjectRemoved(object sender, TuioObjectEventArgs e)
        //{
        //    var obj = e.Object;
        //    lock (this)
        //    {
        //        int existingObject;
        //        if (!objectToInternalId.TryGetValue(obj, out existingObject)) return;

        //        objectToInternalId.Remove(obj);
        //        endObj(existingObject);
        //    }
        //}

        #endregion
    }
}