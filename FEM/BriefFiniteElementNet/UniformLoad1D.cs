﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using BriefFiniteElementNet.Elements;
using MATH;

namespace BriefFiniteElementNet
{
    /// <summary>
    /// Represents a uniform load with specified magnitude which is applying to an <see cref="FrameElement2Node"/> element.
    /// </summary>
    [Serializable]
    public class UniformLoad1D : Load1D
    {
        #region Members

        private double magnitude;

        /// <summary>
        /// Gets or sets the magnitude.
        /// </summary>
        /// <remarks>
        /// Value is magnitude of distributed load, the unit is [N/m]
        /// </remarks>
        /// <value>
        /// The magnitude of distributed load along the member.
        /// </value>
        public double Magnitude
        {
            get { return magnitude; }
            set { magnitude = value; }
        }

        #endregion

        #region Methods

        public Force[] GetGlobalEquivalentNodalLoads(Element element)
        {
            if (element is FrameElement2Node)
            {
                var frElm = element as FrameElement2Node;

                var l = (frElm.EndNode.Location - frElm.StartNode.Location).Length;

                var w = GetLocalDistributedLoad(element as Element1D);

                var localEndForces = new Force[2];

                localEndForces[0] = new Force(w.X*l/2, w.Y*l/2, w.Z*l/2, 0, -w.Z*l*l/12.0, w.Y*l*l/12.0);
                localEndForces[1] = new Force(w.X*l/2, w.Y*l/2, w.Z*l/2, 0, w.Z*l*l/12.0, -w.Y*l*l/12.0);

                localEndForces = CalcUtil.ApplyReleaseMatrixToEndForces(frElm, localEndForces);//applying release matrix to end forces



                for (var i = 0; i < element.Nodes.Length; i++)
                {
                    var frc = localEndForces[i];

                    localEndForces[i] =
                        new Force(
                            frElm.TransformLocalToGlobal(frc.Forces),
                            frElm.TransformLocalToGlobal(frc.Moments));
                }

                return localEndForces;
            }


            return element.GetGlobalEquivalentNodalLoads(this);
        }

        /// <summary>
        /// Gets the local distributed load.
        /// </summary>
        /// <param name="elm">The elm.</param>
        /// <returns></returns>
        /// <remarks>
        /// Gets a vector that its components shows Wx, Wy and Wz in local coordination system of <see cref="elm"/>
        /// </remarks>
        private Vector GetLocalDistributedLoad(Element1D elm)
        {
            if (coordinationSystem == CoordinationSystem.Local)
            {
                return new Vector(
                    direction == LoadDirection.X ? this.magnitude : 0,
                    direction == LoadDirection.Y ? this.magnitude : 0,
                    direction == LoadDirection.Z ? this.magnitude : 0);
            }

            if (elm is FrameElement2Node)
            {
                var frElm = elm as FrameElement2Node;

                var w = new Vector();


                var globalVc = new Vector(
                    direction == LoadDirection.X ? this.magnitude : 0,
                    direction == LoadDirection.Y ? this.magnitude : 0,
                    direction == LoadDirection.Z ? this.magnitude : 0);

                w = frElm.TransformGlobalToLocal(globalVc);

                return w;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the internal force based on only this load at defined position <see cref="x"/>.
        /// </summary>
        /// <param name="elm">The elm.</param>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Force GetInternalForceAt(Element1D elm, double x)
        {
            if (elm is FrameElement2Node)
            {
                var frElm = elm as FrameElement2Node;

                var l = (frElm.EndNode.Location - frElm.StartNode.Location).Length;
                var w = GetLocalDistributedLoad(elm);

                var gf1 = -GetGlobalEquivalentNodalLoads(elm)[0];
                var f1 = new Force();

                f1.Forces = frElm.TransformGlobalToLocal(gf1.Forces);
                f1.Moments = frElm.TransformGlobalToLocal(gf1.Moments);

                var f2 = new Force(new Vector(w.X*x, w.Y*x, w.Z*x), new Vector());

                var buf = f1.Move(new Point(0, 0, 0), new Point(x, 0, 0)) +
                          f2.Move(new Point(x/2, 0, 0), new Point(x, 0, 0));

                return -buf;
            }

            throw new NotImplementedException();
        }

        public override Displacement GetLocalDeformationAt_MC(Element1D elm, double x, bool bConsiderNodalDisplacementOnly = false, bool bConsiderNodalDisplacement = false)
        {
            if (elm is FrameElement2Node)
            {
                var frElm = elm as FrameElement2Node;

                if (frElm.Iz == 0 || frElm.Iy == 0)
                    throw new ArgumentNullException("Moment of inertia is not defined.");

                var l = (frElm.EndNode.Location - frElm.StartNode.Location).Length;
                var w = GetLocalDistributedLoad(elm);

                var gStartDisp = frElm.StartNode.GetNodalDisplacement();
                var gEndDisp = frElm.EndNode.GetNodalDisplacement();

                var lStartDisp = new Displacement(
                    frElm.TransformGlobalToLocal(gStartDisp.Displacements),
                    frElm.TransformGlobalToLocal(gStartDisp.Rotations));

                var lEndDisp = new Displacement(
                    frElm.TransformGlobalToLocal(gEndDisp.Displacements),
                    frElm.TransformGlobalToLocal(gEndDisp.Rotations));

                // Linear displacement in x-location due to displacement of element end nodes
                double dx_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.DX, lStartDisp.DX, x, l);
                double dy_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.DY, lStartDisp.DY, x, l);
                double dz_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.DZ, lStartDisp.DZ, x, l);
                double rx_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.RX, lStartDisp.RX, x, l);
                double ry_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.RY, lStartDisp.RY, x, l);
                double rz_linear_Atx = GetDisplacementAt_x_linear(lEndDisp.RZ, lStartDisp.RZ, x, l);

                var lDisp_x_linear = new Displacement(
                    dx_linear_Atx,
                    dy_linear_Atx,
                    dz_linear_Atx,
                    rx_linear_Atx,
                    ry_linear_Atx,
                    rz_linear_Atx);

                var buf = new Displacement();

                if (bConsiderNodalDisplacement)
                {
                    buf.DX = lDisp_x_linear.DX;
                    buf.DY = lDisp_x_linear.DY + (bConsiderNodalDisplacementOnly ? 0 : GetDeflectionAt_x2(x, l, w.Y, frElm.E, frElm.Iz));
                    buf.DZ = lDisp_x_linear.DZ + (bConsiderNodalDisplacementOnly ? 0 : GetDeflectionAt_x2(x, l, w.Z, frElm.E, frElm.Iy));
                    buf.RX = lDisp_x_linear.RX;
                    buf.RY = lDisp_x_linear.RY; // TODO - not implemented
                    buf.RZ = lDisp_x_linear.RZ; // TODO - not implemented
                }
                else
                {
                    buf.DX = 0;
                    buf.DY = GetDeflectionAt_x2(x, l, w.Y, frElm.E, frElm.Iz);
                    buf.DZ = GetDeflectionAt_x2(x, l, w.Z, frElm.E, frElm.Iy);
                    buf.RX = 0;
                    buf.RY = 0; // TODO - not implemented
                    buf.RZ = 0; // TODO - not implemented
                }

                if (buf.DZ == double.NaN)
                {

                }

                return -buf;
            }

            throw new NotImplementedException();
        }

        private double GetDeflectionAt_x2(double x, double l, double w, double E, double I)
        {
                //return (-w * x / (24f * E * I)) * (MathF.Pow3(l) + 2 * l * MathF.Pow2(x) + MathF.Pow3(x)); // Nespravna rovnica ??
                return ((-w * x * (l - x)) / (24f * E * I)) * (MathF.Pow2(l) + x * (l - x));
        }

        // TODO - refactoring
        private double GetDisplacementAt_x_linear(double dstart, double dend, double x, double l)
        {
            // Linear interpolation (l and x are always positive)
            return MATH.ARRAY.ArrayF.GetLinearInterpolationValuePositive(0, l, dstart, dend, x);
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UniformLoad1D"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude of load.</param>
        /// <param name="direction">The direction of load.</param>
        /// <param name="sys">The coordination system type.</param>
        /// <param name="cse">The load case.</param>
        public UniformLoad1D(double magnitude, LoadDirection direction, CoordinationSystem sys, LoadCase cse)
        {
            this.magnitude = magnitude;
            this.coordinationSystem = sys;
            this.direction = direction;
            this.Case = cse;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UniformLoad1D"/> class.
        /// </summary>
        /// <param name="magnitude">The magnitude.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="sys">The system.</param>
        public UniformLoad1D(double magnitude, LoadDirection direction, CoordinationSystem sys)
        {
            this.magnitude = magnitude;
            this.coordinationSystem = sys;
            this.direction = direction;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UniformLoad1D"/> class.
        /// </summary>
        public UniformLoad1D()
        {
        }

        #endregion

        #region Serialization stuff and constructor

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("magnitude", magnitude);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UniformLoad1D"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        protected UniformLoad1D(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.magnitude = info.GetDouble("magnitude");
        }

        #endregion
    }
}
