using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
    public class FixedAngleJoint : FarseerJoint
    {
        public float BiasFactor;
        public float MaxImpulse;
        public float Softness;
        private float _bias;
        private float _jointError;
        private float _massFactor;
        private float _targetAngle;

        public FixedAngleJoint(Body bodyA)
            : base(bodyA)
        {
            JointType = JointType.FixedAngle;
            TargetAngle = 0;
            BiasFactor = .2f;
            Softness = 0f;
            MaxImpulse = float.MaxValue;
        }

        public float TargetAngle
        {
            get { return _targetAngle; }
            set
            {
                if (value != _targetAngle)
                {
                    _targetAngle = value;
                    WakeBodies();
                }
            }
        }

        public override FVector2 WorldAnchorA
        {
            get { return BodyA.Position; }
        }

        public override FVector2 WorldAnchorB
        {
            get { return BodyA.Position; }
            set { Debug.Assert(false, "You can't set the world anchor on this joint type."); }
        }

        public override FVector2 GetReactionForce(float inv_dt)
        {
            //TODO
            //return _inv_dt * _impulse;
            return FVector2.Zero;
        }

        public override float GetReactionTorque(float inv_dt)
        {
            return 0;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            _jointError = BodyA.Sweep.A - TargetAngle;

            _bias = -BiasFactor * data.step.inv_dt * _jointError;

            _massFactor = (1 - Softness) / (BodyA.InvI);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            float p = (_bias - BodyA.AngularVelocity) * _massFactor;
            BodyA.AngularVelocity += BodyA.InvI * Math.Sign(p) * Math.Min(Math.Abs(p), MaxImpulse);
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            //no position solving for this joint
            return true;
        }
    }
}