cmake_minimum_required(VERSION 3.5)
project(arq)

#set(CMAKE_CXX_STANDARD 17)
#set(CMAKE_CXX_STANDARD_REQUIRED ON)

#set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)
#find_package(Libstored COMPONENTS ZeroMQ REQUIRED)

# Point to the directory where you have cloned libstored.
set(LIBSTORED_CLONE ${PROJECT_SOURCE_DIR}/../extern/libstored)
list(APPEND CMAKE_MODULE_PATH ${LIBSTORED_CLONE}/cmake)

# Make sure this venv is already created and have the required packages installed.
set(PYTHON_EXECUTABLE ${PROJECT_SOURCE_DIR}/.venv/bin/python)
message(STATUS "Using Python from ${PYTHON_EXECUTABLE}")

# Add ZMQ
list(APPEND CMAKE_MODULE_PATH ${LIBSTORED_CLONE}/dist/common)
find_package(ZeroMQ REQUIRED)
set(LIBSTORED_HAVE_LIBZMQ ON)

# Include libstored functions
include(libstored)
message(STATUS "Using Libstored from ${LIBSTORED_SOURCE_DIR}")

# Create executable
add_executable(arq main.cpp)
libstored_generate(
    TARGET arq STORES ArqStore.st DESTINATION
	${CMAKE_CURRENT_BINARY_DIR}
)

#install(TARGETS arq RUNTIME)