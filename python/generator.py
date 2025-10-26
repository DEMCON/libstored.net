#!/usr/bin/env python3
# SPDX-FileCopyrightText: 2025 Guus Kuiper
#
# SPDX-License-Identifier: MIT

import argparse
import importlib
import jinja2
import os
import sys
import struct
import types
import json
import base64
import yaml
from dataclasses import dataclass, asdict
from typing import Iterable, Protocol, Union, runtime_checkable

@runtime_checkable
class MetaObjectMeta(Protocol):
    # Defines the expected interface for a meta object class

    def __init__(self, name, cname, type, ctype, size, isfunction, f, offset, init, axi):
        self.name = name
        self.cname = cname
        self.type = type
        self.ctype = ctype
        self.size = size
        self.offset = offset
        self.isfunction = isfunction


    def _asdict(self) -> dict:
        ...

    def __repr__(self):
        return f'MetaObjectMeta(name={self.name}, cname={self.cname}, type={self.type})'


@runtime_checkable
class MetaProtocol(Protocol):
    # Defines the expected interface for a meta protocol class

    @property
    def name(self) -> str:
        ...

    @property
    def hash(self) -> str:
        ...

    @property
    def objects(self) -> Iterable[MetaObjectMeta]:
        ...

    @property
    def functions(self) -> Iterable[MetaObjectMeta]:
        ...

    @property
    def variables(self) -> Iterable[MetaObjectMeta]:
        ...

@dataclass
class StoreVariable:
    name: str
    cname: str
    type: str
    size: int
    offset: int
    init: Union[str, float, int, None]

@dataclass
class StoreModel:
    name: str
    hash: str
    littleEndian: bool
    variables: list[StoreVariable]

def cstr(s):
    bs = str(s).encode()
    cs = '"'
    for b in bs:
        if b < 32 or b >= 127:
            cs += f'\\x{b:02x}'
        else:
            cs += chr(b)
    return cs + '"'

def cstypes(o):
        return {
            'bool': 'bool',
            'int8': 'sbyte',
            'uint8': 'byte',
            'int16': 'short',
            'uint16': 'ushort',
            'int32': 'int',
            'uint32': 'uint',
            'int64': 'long',
            'uint64': 'ulong',
            'float': 'float',
            'double': 'double',
            'ptr32': 'uint',
            'ptr64': 'ulong',
            'blob': 'byte[]',
            'string': 'string'
    }[o]

def csetypes(o : str):
    t = {
        'bool': 'Types.Bool',
        'int8': 'Types.Int8',
        'uint8': 'Types.Uint8',
        'int16': 'Types.Int16',
        'uint16': 'Types.Uint16',
        'int32': 'Types.Int32',
        'uint32': 'Types.Uint32',
        'int64': 'Types.Int64',
        'uint64': 'Types.Uint64',
        'float': 'Types.Float',
        'double': 'Types.Double',
        'ptr32': 'Types.Pointer32',
        'ptr64': 'Types.Pointer64',
        'blob': 'Types.Blob',
        'string': 'Types.String'
    }[o]

    return t

def csprop(cname: str) -> str:
    # Split by underscores, capitalize each part, and join
    parts = cname.split('_')
    prefix = '_' if not parts[0] else ''
    return prefix + ''.join(capitalize_first(part) for part in cname.split('_'))

def csfield(cname: str) -> str:
    # Split by underscores, capitalize each part except the first, then join and prefix with '_'
    parts = cname.split('_')
    prefix = '_' if not parts[0] else ''
    field_name = parts[0] + ''.join(capitalize_first(part) for part in parts[1:])
    return f'{prefix}_{field_name}'

def capitalize_first(s: str) -> str:
    """
    Capitalizes only the first character of the string, leaving the rest unchanged.
    """
    return s[:1].upper() + s[1:] if s else s

def is_variable(o):
    return not o.isfunction and o.type != 'blob' and o.type != 'string'

def encode_string(x):
    s = x.encode()
    assert len(s) <= x.size
    return s + bytes([0] * (x.size - len(s)))

def bytes_to_hex_list(data) -> str:
    return ', '.join(f'0x{b:02x}' for b in data)

def encode_initial(xs : Iterable[MetaObjectMeta], littleEndian=True) -> str:
    endian = '<' if littleEndian else '>'
    res = bytearray()
    for x in xs:
        if x.init is None:
            break
        padding = x.offset - len(res)
        if padding > 0:
            # Fill with zeros until the offset
            res += bytearray([0] * padding)  # Fill with zeros until the offset

        res += encode(x, littleEndian)
    return bytes_to_hex_list(res)

def encode(x, littleEndian=True):
    endian = '<' if littleEndian else '>'
    res = {
            'bool': lambda x: struct.pack(endian + '?', not x in [False, 'false', 0]),
            'int8': lambda x: struct.pack(endian + 'b', int(x)),
            'uint8': lambda x: struct.pack(endian + 'B', int(x)),
            'int16': lambda x: struct.pack(endian + 'h', int(x)),
            'uint16': lambda x: struct.pack(endian + 'H', int(x)),
            'int32': lambda x: struct.pack(endian + 'i', int(x)),
            'uint32': lambda x: struct.pack(endian + 'I', int(x)),
            'int64': lambda x: struct.pack(endian + 'q', int(x)),
            'uint64': lambda x: struct.pack(endian + 'Q', int(x)),
            'float': lambda x: struct.pack(endian + 'f', float(x)),
            'double': lambda x: struct.pack(endian + 'd', float(x)),
            'ptr32': lambda x: struct.pack(endian + 'L', int(x)),
            'ptr64': lambda x: struct.pack(endian + 'Q', int(x)),
            'blob': lambda x: bytearray(x),
            'string': lambda s: s.encode() + bytes([0] * (x.size - len(s.encode())))
    }[x.type](x.init)
    return res


def load_class_from_file(filepath: str, classname: str) -> MetaProtocol:
    spec = importlib.util.spec_from_file_location(classname, filepath)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    cls = getattr(module, classname)
    return cls

def load_class_from_source(source_code: str) -> MetaProtocol:
    module = types.ModuleType("dynamic_module")
    exec(source_code, module.__dict__)
    for name, obj in module.__dict__.items():
        if isinstance(obj, object) and name.endswith('Meta') and not name.endswith('ObjectMeta'):
            return obj
    return None

def generate_model(meta : MetaProtocol) -> StoreModel:

    variables = []
    for v in sorted(meta.variables, key=lambda x: x.offset):
        init = v.init

        # Fix for bool initializations libstored also accepts int > 1, so convert to bool true / false
        if v.type == 'bool' and v.init is not None:
            init = bool(v.init)

        sv = StoreVariable(
            name=v.name,
            cname=v.cname,
            type=v.type,
            size=v.size,
            offset=v.offset,
            init=init
        )
        variables.append(sv)

    store_model = StoreModel(
        name=meta.name,
        hash=meta.hash,
        littleEndian=True,
        variables=variables
    )

    return store_model

def generate(meta : MetaProtocol, tmpl_filename : str) -> tuple[str, str, str]:

    # Validate the meta object
    if not isinstance(meta, MetaProtocol):
        raise TypeError("Expected a MetaProtocol instance")

    if not isinstance(next(meta.variables), MetaObjectMeta):
        raise TypeError("Expected a MetaObjectMeta instances")
    
    model = generate_model(meta)

    jenv = jinja2.Environment(
        loader=jinja2.FileSystemLoader(os.path.dirname(tmpl_filename)),
        trim_blocks=True)

    jenv.filters['cstr'] = cstr
    jenv.filters['cstypes'] = cstypes
    jenv.filters['csetypes'] = csetypes
    jenv.filters['csprop'] = csprop
    jenv.filters['csfield'] = csfield
    jenv.filters['encode_initial'] = encode_initial
    jenv.tests['variable'] = is_variable

    tmpl = jenv.get_template(os.path.basename(tmpl_filename))

    source = tmpl.render(store=meta)

    js = json.dumps(asdict(model), indent=2)

    # force all Python strings to be emitted as quoted scalars in YAML
    class QuotedSafeDumper(yaml.SafeDumper):
        pass

    def _represent_str_quoted(dumper, data):
        # use double quotes for string scalars
        return dumper.represent_scalar('tag:yaml.org,2002:str', str(data), style='"')

    QuotedSafeDumper.add_representer(str, _represent_str_quoted)

    # dump using the custom dumper; keep key order stable
    yml = yaml.dump(asdict(model), Dumper=QuotedSafeDumper, sort_keys=False)

    return source, js, yml

def generate_cs_meta_py(meta_py_code: str) -> str:
    """
    Generates a C# store file from the provided python meta source code.
    """

    # Load the class from the source code
    cls = load_class_from_source(meta_py_code)
    if cls is None:
        raise ValueError("No class found in the provided source code.")

    # Create an instance of the class
    meta_instance = cls()

    template = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'store.cs.tmpl')

    cs, _ = generate(meta_instance, template)

    return cs


def main():
    parser = argparse.ArgumentParser(description='Generator using store meta data')
    script_dir = os.path.dirname(os.path.abspath(__file__))

    parser.add_argument('-m', '--meta', type=str, default=os.path.join(script_dir, 'TestStoreMeta.py'), help='path to <store>Meta.py as input', dest="meta")
    parser.add_argument('-t', '--template', type=str, default=os.path.join(script_dir, 'store.cs.tmpl'), help='path to jinja2 template META is to be applied to', dest="template")
    parser.add_argument('-o', '--output', type=str, help='output file for jinja2 generated content', dest="output")

    args = parser.parse_args()

    file = os.path.abspath(args.meta)
    template = os.path.abspath(args.template)

    module_name = os.path.splitext(os.path.basename(file))[0]
    loaded_meta_class = load_class_from_file(file, module_name)

    meta = loaded_meta_class()  # Create instance

    output_name = args.output if args.output else os.path.join(script_dir, f'{meta.name}.g.cs')
    output_name = os.path.abspath(output_name)

    cs, json, yml = generate(meta, template)

    output_dir = os.path.dirname(output_name)
    if not os.path.exists(output_dir):
        os.mkdir(output_dir)

    with open(output_name, 'w') as f:
        f.write(cs)

    with open(output_name + '.json', 'w') as jf:
        jf.write(json)

    with open(output_name + '.yml', 'w') as yf:
        yf.write(yml)


if __name__ == "__main__":
    main()
