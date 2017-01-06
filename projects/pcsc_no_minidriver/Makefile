PROGR:=$(TARGET_DIR)/$(TARGET).exe

SRCS:=$(SOURCE_DIR)/pcsc_no_minidriver.c

_OBJS:=$(patsubst %.c,%.obj,$(SRCS))
OBJS:=$(subst $(SOURCE_DIR),$(OBJECT_DIR),$(_OBJS))

all: $(PROGR)

$(PROGR): $(OBJS)
	@echo # Linking $(notdir $@)
	$(LD) /out:$@ $(LDFLAGS) $^  
  
$(OBJECT_DIR)/%.obj: $(SOURCE_DIR)/%.c
	- @mkdir.exe -p $(subst /,\,$(dir $@))
	@echo # Compiling $(notdir $<)
	$(CC) $(CCFLAGS) /TC /Fo$@ $<



