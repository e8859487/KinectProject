namespace MotionFSM
{
    using System;
    using System.Activities;

    public interface IWorkflowView
    {
        #region Public Methods and Operators
        /// <summary>
        /// The on idle.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        void OnIdle(WorkflowApplicationIdleEventArgs args);

        /// <summary>
        /// The on unhandled exception.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The unhandled exception action.
        /// </returns>
        UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args);
        #endregion
    }
}
