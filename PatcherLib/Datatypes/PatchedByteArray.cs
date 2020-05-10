/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    FFTPatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FFTPatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FFTPatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using PatcherLib.Iso;
using System.IO;

namespace PatcherLib.Datatypes
{
    public class STRPatchedByteArray : LazyLoadedPatchedByteArray 
    {
        public string Filename { get; private set; }

        public override byte[] GetBytes()
        {
            return File.ReadAllBytes( Filename );
        }

        public STRPatchedByteArray(PsxIso.Sectors sector, string filename)
            : base(sector, 0 )
        {
            this.Filename = filename;
        }
    }

    public class InputFilePatch : LazyLoadedPatchedByteArray
    {
        public string Filename { get; private set; }

        public uint ExpectedLength { get; private set; }

        public void SetFilename( string filename )
        {
            string myfile = Path.GetFullPath( filename );
            if ( !File.Exists( myfile ) )
            {
                Filename = null;
                throw new FileNotFoundException();
            }
            else if ( new FileInfo( filename ).Length != ExpectedLength )
            {
                throw new ArgumentException( "file is wrong length" );
            }
            else
            {
                Filename = filename;
            }
        }

        public override byte[] GetBytes()
        {
            if ( string.IsNullOrEmpty( Filename ) )
            {
                throw new InvalidOperationException( "Filename not yet set" );
            }
            else
            {
                return File.ReadAllBytes( Filename );
            }
        }

        public InputFilePatch( PsxIso.Sectors sector, uint offset, uint expectedLength )
            : base( sector, offset )
        {
            ExpectedLength = expectedLength;
        }
    }

    public abstract class LazyLoadedPatchedByteArray : PatchedByteArray
    {
        public override abstract byte[] GetBytes();

        public LazyLoadedPatchedByteArray(PsxIso.Sectors sector, long offset )
            : this((int)sector, offset )
        {
            SectorEnum = sector;
        }

        public LazyLoadedPatchedByteArray(PspIso.Sectors sector, long offset )
            : this((int)sector, offset )
        {
            SectorEnum = sector;
        }

        public LazyLoadedPatchedByteArray(FFTPack.Files file, long offset )
            : this((int)file, offset )
        {
            SectorEnum = file;
        }

        public LazyLoadedPatchedByteArray(Enum fileOrSector, long offset )
            : this(-1, offset )
        {
            SectorEnum = fileOrSector;
            Type t = fileOrSector.GetType();
            if (t == typeof(PspIso.Sectors))
            {
                Sector = (int)((PspIso.Sectors)fileOrSector);
            }
            else if (t == typeof(FFTPack.Files))
            {
                Sector = (int)((FFTPack.Files)fileOrSector);
            }
            else if (t == typeof(PsxIso.Sectors))
            {
                Sector = (int)((PsxIso.Sectors)fileOrSector);
            }
            else
            {
                throw new ArgumentException("fileOrSector has incorrect type");
            }
        }

        public LazyLoadedPatchedByteArray(int sector, long offset )
            : base(sector, offset)
        {
        }

    }

    public class PatchedByteArray
    {
		#region Public Properties (4) 

        public virtual byte[] GetBytes()
        {
            return bytes;
        }
        public void SetBytes(byte[] bytes)
        {
            this.bytes = bytes;
        }

        private byte[] bytes;

        public long Offset { get; protected set; }

        public int Sector { get; protected set; }

        public Enum SectorEnum { get; protected set; }

        public bool IsAsm { get; set; }
        public bool MarkedAsData { get; set; }
        public string AsmText { get; set; }
        public long RamOffset { get; set; }
        public string ErrorText { get; set; }
        public string Label { get; set; }

		#endregion Public Properties 

		#region Constructors (4) 

        public PatchedByteArray( PsxIso.Sectors sector, long offset, byte[] bytes )
            : this( (int)sector, offset, bytes )
        {
            SectorEnum = sector;
        }

        public PatchedByteArray( PspIso.Sectors sector, long offset, byte[] bytes )
            : this( (int)sector, offset, bytes )
        {
            SectorEnum = sector;
        }

        public PatchedByteArray( FFTPack.Files file, long offset, byte[] bytes )
            : this( (int)file, offset, bytes )
        {
            SectorEnum = file;
        }

        public PatchedByteArray(Enum fileOrSector, long offset, byte[] bytes)
            : this(-1, offset, bytes)
        {
            SectorEnum = fileOrSector;
            Type t = fileOrSector.GetType();
            if (t == typeof(PspIso.Sectors))
            {
                Sector = (int)((PspIso.Sectors)fileOrSector);
            }
            else if (t == typeof(FFTPack.Files))
            {
                Sector = (int)((FFTPack.Files)fileOrSector);
            }
            else if (t == typeof(PsxIso.Sectors))
            {
                Sector = (int)((PsxIso.Sectors)fileOrSector);
            }
            else
            {
                throw new ArgumentException("fileOrSector has incorrect type");
            }
        }

        public PatchedByteArray(int sector, long offset, byte[] bytes)
            : this(sector, offset)
        {
            this.bytes = bytes;
        }

        protected PatchedByteArray(int sector, long offset)
        {
            Sector = sector;
            Offset = offset;
        }

		#endregion Constructors 

        public bool IsPatchEqual(PatchedByteArray patchedByteArray)
        {
            if (patchedByteArray == null)
                return false;
            
            if ((Sector != patchedByteArray.Sector) || (Offset != patchedByteArray.Offset))
                return false;

            byte[] compareBytes = patchedByteArray.GetBytes();

            if ((bytes == null) || (compareBytes == null))
                return false;

            if (bytes.Length != compareBytes.Length)
                return false;

            for (int index = 0; index < bytes.Length; index++)
            {
                if (bytes[index] != compareBytes[index])
                    return false;
            }

            return true;
        }
    }
}
