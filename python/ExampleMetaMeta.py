# SPDX-FileCopyrightText: 2020-2023 Jochem Rutgers
#
# SPDX-License-Identifier: MPL-2.0

from collections import namedtuple

ExampleMetaObjectMeta = namedtuple('ExampleMetaObjectMeta', ['name', 'cname', 'type', 'ctype', 'size', 'isfunction', 'f', 'offset', 'init', 'axi'])

class ExampleMetaMeta(object):
    def __init__(self):
        self._objects = [
            ExampleMetaObjectMeta('some int', 'some_int', 'int32', 'int32_t', 4, False, None, 16, int(42), 0),
            ExampleMetaObjectMeta('a double', 'a_double', 'double', 'double', 8, False, None, 0, float('1.618'), None),
            ExampleMetaObjectMeta('world', 'world', 'string', 'char', 7, False, None, 8, 'hello', None)]

    @property
    def name(self):
        return 'ExampleMeta'

    @property
    def hash(self):
        return '96cc75d260a03f4931816b2bc0824eed28faa9ae'

    @property
    def objects(self):
        return self._objects

    @property
    def functions(self):
        return filter(lambda o: o.isfunction, self._objects)

    @property
    def variables(self):
        return filter(lambda o: not o.isfunction, self._objects)

    def __len__(self):
        return len(self._objects)

    def __getitem__(self, key):
        return next(filter(lambda o: o.name == key, self._objects))

    def __getattr__(self, name):
        return next(filter(lambda o: o.cname == name, self._objects))

    def __iter__(self):
        return iter(self._objects)