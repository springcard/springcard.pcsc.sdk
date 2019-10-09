#
# Here's the compiler command line
# replace /home/projects/common by the actual path to the shared sources
#
CFLAGS += -Wall -Wextra -Werror -I/usr/include/PCSC -I/usr/local/include/PCSC -I/home/projects/common

#
# Here's the linker command line
#
LDFLAGS += -L../../../../library/linux/bin
LDLIBS  += -lpcsclite

#CCFLAGS+=/D _DEBUG

all:\
	./bin/ref_calypso_pcsc \
	./bin/ref_desfire_ev1_pcsc \
	./bin/ref_desfire_ev0_pcsc \
	./bin/ref_ledbuz_pcsc \
	./bin/ref_memory_pcsc \
	./bin/ref_mifare_pcsc \
	./bin/ref_mifplus_pcsc \
	./bin/ref_mifulc_pcsc

clean:
	rm -f ./obj/*.o
	rm -f ./bin/*

./bin/ref_calypso_pcsc: obj/ref_calypso_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -o $@

./bin/ref_desfire_ev1_pcsc: obj/ref_desfire_ev1_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -lpcsc_desfire -o $@

./bin/ref_desfire_ev0_pcsc: obj/ref_desfire_ev0_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -lpcsc_desfire -o $@

./bin/ref_mifplus_pcsc: obj/ref_mifplus_pcsc.o obj/pcsc_helpers.o obj/mifare_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -lpcsc_mifplus -o $@

./bin/ref_ledbuz_pcsc: obj/ref_ledbuz_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -o $@

./bin/ref_memory_pcsc: obj/ref_memory_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -o $@

./bin/ref_mifare_pcsc: obj/ref_mifare_pcsc.o obj/pcsc_helpers.o obj/mifare_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -o $@

./bin/ref_mifulc_pcsc: obj/ref_mifulc_pcsc.o obj/pcsc_helpers.o
	$(CC) $(LDFLAGS) $^ $(LDLIBS) -lpcsc_mifulc -o $@

obj/%.o: ../%.c
	$(CC) $(CFLAGS) -c -o $@ $<
