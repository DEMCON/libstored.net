mkdir -p build
cd build
cmake ..
cmake --build .
# Regenerate CMake after code generation
cmake ..
cmake --build . --target install