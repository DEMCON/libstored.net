# SPDX-FileCopyrightText: 2020-2023 Jochem Rutgers
#
# SPDX-License-Identifier: MPL-2.0

from collections import namedtuple

TestStoreObjectMeta = namedtuple('TestStoreObjectMeta', ['name', 'cname', 'type', 'ctype', 'size', 'isfunction', 'f', 'offset', 'init', 'axi'])

class TestStoreMeta(object):
    def __init__(self):
        self._objects = [
            TestStoreObjectMeta('default int8', 'default_int8', 'int8', 'int8_t', 1, False, None, 268, None, 0),
            TestStoreObjectMeta('default int16', 'default_int16', 'int16', 'int16_t', 2, False, None, 264, None, 4),
            TestStoreObjectMeta('default int32', 'default_int32', 'int32', 'int32_t', 4, False, None, 184, None, 8),
            TestStoreObjectMeta('default int64', 'default_int64', 'int64', 'int64_t', 8, False, None, 128, None, None),
            TestStoreObjectMeta('default uint8', 'default_uint8', 'uint8', 'uint8_t', 1, False, None, 269, None, 12),
            TestStoreObjectMeta('default uint16', 'default_uint16', 'uint16', 'uint16_t', 2, False, None, 266, None, 16),
            TestStoreObjectMeta('default uint32', 'default_uint32', 'uint32', 'uint32_t', 4, False, None, 188, None, 20),
            TestStoreObjectMeta('default uint64', 'default_uint64', 'uint64', 'uint64_t', 8, False, None, 136, None, None),
            TestStoreObjectMeta('default float', 'default_float', 'float', 'float', 4, False, None, 192, None, 24),
            TestStoreObjectMeta('default double', 'default_double', 'double', 'double', 8, False, None, 144, None, None),
            TestStoreObjectMeta('default bool', 'default_bool', 'bool', 'bool', 1, False, None, 270, None, 28),
            TestStoreObjectMeta('default ptr32', 'default_ptr32', 'ptr32', 'void*', 4, False, None, 196, None, 32),
            TestStoreObjectMeta('default ptr64', 'default_ptr64', 'ptr64', 'void*', 8, False, None, 152, None, None),
            TestStoreObjectMeta('default blob', 'default_blob', 'blob', 'void', 5, False, None, 176, None, None),
            TestStoreObjectMeta('default string', 'default_string', 'string', 'char', 10, False, None, 112, None, None),
            TestStoreObjectMeta('init decimal', 'init_decimal', 'int32', 'int32_t', 4, False, None, 24, int(42), 36),
            TestStoreObjectMeta('init negative', 'init_negative', 'int32', 'int32_t', 4, False, None, 28, int(-42), 40),
            TestStoreObjectMeta('init hex', 'init_hex', 'int32', 'int32_t', 4, False, None, 32, int(84), 44),
            TestStoreObjectMeta('init bin', 'init_bin', 'int32', 'int32_t', 4, False, None, 36, int(5), 48),
            TestStoreObjectMeta('init true', 'init_true', 'bool', 'bool', 1, False, None, 100, True, 52),
            TestStoreObjectMeta('init false', 'init_false', 'bool', 'bool', 1, False, None, 271, None, 56),
            TestStoreObjectMeta('init bool 0', 'init_bool_0', 'bool', 'bool', 1, False, None, 272, None, 60),
            TestStoreObjectMeta('init bool 10', 'init_bool_10', 'bool', 'bool', 1, False, None, 101, int(10), 64),
            TestStoreObjectMeta('init float 0', 'init_float_0', 'float', 'float', 4, False, None, 200, None, 68),
            TestStoreObjectMeta('init float 1', 'init_float_1', 'float', 'float', 4, False, None, 40, int(1), 72),
            TestStoreObjectMeta('init float 3.14', 'init_float_3_14', 'float', 'float', 4, False, None, 44, float('3.14'), 76),
            TestStoreObjectMeta('init float -4000', 'init_float_4000', 'float', 'float', 4, False, None, 48, float('-4000.0'), 80),
            TestStoreObjectMeta('init float nan', 'init_float_nan', 'float', 'float', 4, False, None, 52, float('nan'), 84),
            TestStoreObjectMeta('init float inf', 'init_float_inf', 'float', 'float', 4, False, None, 56, float('inf'), 88),
            TestStoreObjectMeta('init float neg inf', 'init_float_neg_inf', 'float', 'float', 4, False, None, 60, float('-inf'), 92),
            TestStoreObjectMeta('init string', 'init_string', 'string', 'char', 8, False, None, 0, '.nan', None),
            TestStoreObjectMeta('init string empty', 'init_string_empty', 'string', 'char', 8, False, None, 160, None, None),
            TestStoreObjectMeta('f read/write', 'f_read__write', 'double', 'double', 8, True, 1, None, None, None),
            TestStoreObjectMeta('f read-only', 'f_read_only', 'uint16', 'uint16_t', 2, True, 2, None, None, None),
            TestStoreObjectMeta('f write-only', 'f_write_only', 'string', 'char', 4, True, 3, None, None, None),
            TestStoreObjectMeta('array bool[0]', 'array_bool_0', 'bool', 'bool', 1, False, None, 102, True, 96),
            TestStoreObjectMeta('array bool[1]', 'array_bool_1', 'bool', 'bool', 1, False, None, 103, True, 100),
            TestStoreObjectMeta('array bool[2]', 'array_bool_2', 'bool', 'bool', 1, False, None, 273, None, 104),
            TestStoreObjectMeta('array string[0]', 'array_string_0', 'string', 'char', 4, False, None, 204, None, 108),
            TestStoreObjectMeta('array string[1]', 'array_string_1', 'string', 'char', 4, False, None, 212, None, 112),
            TestStoreObjectMeta('array string[2]', 'array_string_2', 'string', 'char', 4, False, None, 220, None, 116),
            TestStoreObjectMeta('array f int[0]', 'array_f_int_0', 'int32', 'int32_t', 4, True, 4, None, None, None),
            TestStoreObjectMeta('array f int[1]', 'array_f_int_1', 'int32', 'int32_t', 4, True, 5, None, None, None),
            TestStoreObjectMeta('array f int[2]', 'array_f_int_2', 'int32', 'int32_t', 4, True, 6, None, None, None),
            TestStoreObjectMeta('array f int[3]', 'array_f_int_3', 'int32', 'int32_t', 4, True, 7, None, None, None),
            TestStoreObjectMeta('array f blob[0]', 'array_f_blob_0', 'blob', 'void', 2, True, 8, None, None, None),
            TestStoreObjectMeta('array f blob[1]', 'array_f_blob_1', 'blob', 'void', 2, True, 9, None, None, None),
            TestStoreObjectMeta('array single', 'array_single', 'float', 'float', 4, False, None, 64, int(3), 120),
            TestStoreObjectMeta('scope/inner bool', 'scope__inner_bool', 'bool', 'bool', 1, False, None, 274, None, 124),
            TestStoreObjectMeta('scope/inner int', 'scope__inner_int', 'int32', 'int32_t', 4, False, None, 228, None, 128),
            TestStoreObjectMeta('some other scope/some other inner bool', 'some_other_scope__some_other_inner_bool', 'bool', 'bool', 1, False, None, 275, None, 132),
            TestStoreObjectMeta('value with unit (km/s)', 'value_with_unit_km__s', 'float', 'float', 4, False, None, 232, None, 136),
            TestStoreObjectMeta('value with complex unit (J/s/m^2)', 'value_with_complex_unit_J__s__m_2', 'float', 'float', 4, False, None, 236, None, 140),
            TestStoreObjectMeta('value with abiguous unit (m/s)', 'value_with_abiguous_unit_m__s', 'float', 'float', 4, False, None, 240, None, 144),
            TestStoreObjectMeta('value with abiguous unit (m/h)', 'value_with_abiguous_unit_m__h', 'float', 'float', 4, False, None, 244, None, 148),
            TestStoreObjectMeta('amp/input', 'amp__input', 'float', 'float', 4, False, None, 248, None, 152),
            TestStoreObjectMeta('amp/enable', 'amp__enable', 'bool', 'bool', 1, False, None, 104, True, 156),
            TestStoreObjectMeta('amp/gain', 'amp__gain', 'float', 'float', 4, False, None, 68, int(2), 160),
            TestStoreObjectMeta('amp/offset', 'amp__offset', 'float', 'float', 4, False, None, 72, float('0.5'), 164),
            TestStoreObjectMeta('amp/low', 'amp__low', 'float', 'float', 4, False, None, 76, int(-1), 168),
            TestStoreObjectMeta('amp/high', 'amp__high', 'float', 'float', 4, False, None, 80, int(10), 172),
            TestStoreObjectMeta('amp/override', 'amp__override', 'float', 'float', 4, False, None, 84, float('nan'), 176),
            TestStoreObjectMeta('amp/output', 'amp__output', 'float', 'float', 4, False, None, 252, None, 180),
            TestStoreObjectMeta('small amp/gain', 'small_amp__gain', 'float', 'float', 4, False, None, 88, float('3.5'), 184),
            TestStoreObjectMeta('small amp/override', 'small_amp__override', 'float', 'float', 4, False, None, 92, float('nan'), 188),
            TestStoreObjectMeta('small amp/output', 'small_amp__output', 'float', 'float', 4, False, None, 256, None, 192),
            TestStoreObjectMeta('ambiguous amp/gain', 'ambiguous_amp__gain', 'float', 'float', 4, False, None, 96, int(-1), 196),
            TestStoreObjectMeta('ambiguous amp/enable', 'ambiguous_amp__enable', 'bool', 'bool', 1, False, None, 276, None, 200),
            TestStoreObjectMeta('ambiguous amp/output', 'ambiguous_amp__output', 'float', 'float', 4, False, None, 260, None, 204),
            TestStoreObjectMeta('double amp/gain', 'double_amp__gain', 'double', 'double', 8, False, None, 16, int(-3), None)]

    @property
    def name(self):
        return 'TestStore'

    @property
    def hash(self):
        return 'b0bba06dabc9dff2a4c18c4ebdcc39e1418575b4'

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