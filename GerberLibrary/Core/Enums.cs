using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerberLibrary.Core
{
    public enum BoardLayer
    {
        Silk,
        Copper,
        Paste,
        SolderMask,
        Carbon,
        Drill,
        Outline,
        Mill,
        Unknown
    }

    public enum BoardSide
    {
        Top,
        Bottom,
        Internal,
        Both,
        Unknown,
        Internal1,
        Internal2
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
