using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class MessageReader : BinaryReader
{
    public MessageReader(Stream stream)
        : base(stream)
    {

    }

    public override short ReadInt16()
    {
        return base.ReadInt16();
    }
}
