using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core
{
    public enum BoardLayer
    {
        Notes,
        Assembly,
        Utility,
        Carbon,
        Silk,
        Paste,
        SolderMask,
        Copper,
        Drill,
        Outline,
        Mill,
        Unknown,
        Any
    }

    public enum BoardSide
    {
        Top,
        Bottom,
        Internal,
        Both,
        Unknown,
        Internal1,
        Internal2,
        Internal3,
        Internal4,
        Internal5,
        Internal6,
        Internal7,
        Internal8,
        Internal9,
        Internal10,
        Internal11,
        Internal12,
        Internal13,
        Internal14,
        Internal15,
        Internal16,
        Internal17,
        Internal18,
        Internal19,
        Internal20,
        Either
    }

    public enum InterpolationMode
    {
        Linear,
        ClockWise,
        CounterClockwise
    }

    public enum Quadrant
    {
        xposypos,
        xnegypos,
        xnegyneg,
        xposyneg

    }

    public enum GerberQuadrantMode
    {
        Multi,
        Single
    }

    public enum BoardFileType
    {
        Gerber,
        Drill,
        Unsupported
    }
}
