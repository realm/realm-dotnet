
CXX=clang++
FLAGS=-Icore/include -Lcore -lrealm -lc++ -std=gnu++11 -stdlib=libc++ -m32 -framework CoreFoundation
 
help: ## Show this help.
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

clean:
	rm -fr build/*
 
libwrappers.dylib:
	$(CXX) $(FLAGS) wrappers.cpp -dynamiclib -o libwrappers.dylib 
 
.PHONY: all help clean test


