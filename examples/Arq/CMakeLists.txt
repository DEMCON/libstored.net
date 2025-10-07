cmake_minimum_required(VERSION 3.10)
project(ArqExample)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} ${CMAKE_CURRENT_SOURCE_DIR}/cmake)
find_package(Libstored COMPONENTS ZeroMQ REQUIRED)

add_executable(arq main.cpp)
libstored_generate(TARGET arq STORES ArqStore.st)
