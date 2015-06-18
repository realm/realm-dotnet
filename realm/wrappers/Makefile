
CXX=clang++
FLAGS=-std=c++11 -stdlib=libc++
all:
 
help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean:
  rm -fr build/*
 
test:
  $(CXX) $(FLAGS) src/test.cpp -o build/test.bin -Isrc/
 
.PHONY: all help clean test


