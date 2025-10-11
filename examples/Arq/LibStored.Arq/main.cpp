#include "ArqStore.h"

#include <stdlib.h>
#include <stored>
#include <string.h>

static stored::Synchronizer synchronizer;

class ArqStore
	: public STORE_T(ArqStore, stored::Synchronizable, stored::ArqStoreBase) {
	STORE_CLASS(ArqStore, stored::Synchronizable, stored::ArqStoreBase)
public:
	ArqStore() is_default
};

static ArqStore store;

/*!
 * \brief Simulate a lossy channel.
 *
 * Depending on the bit error rate (ber) set in the store, bits are flipped.
 * Moreover, it allows to set an MTU via the store.
 */
class LossyChannel : public stored::ProtocolLayer {
	STORED_CLASS_NOCOPY(LossyChannel)
public:
	typedef stored::ProtocolLayer base;

	LossyChannel(ProtocolLayer* up = nullptr, ProtocolLayer* down = nullptr)
		: base(up, down)
	{}

	virtual ~LossyChannel() override is_default

	virtual void decode(void* buffer, size_t len) override
	{
		char* buffer_ = static_cast<char*>(buffer);
		for(size_t i = 0; i < len; i++)
			buffer_[i] = lossyByte(buffer_[i]);

		//printBuffer(buffer, len, "> ");
		base::decode(buffer, len);
	}

	virtual void encode(void const* buffer, size_t len, bool last = true) override
	{
		// cppcheck-suppress allocaCalled
		char* buffer_ = (char*)alloca(len);
		for(size_t i = 0; i < len; i++)
			buffer_[i] = lossyByte(static_cast<char const*>(buffer)[i]);

		//printBuffer(buffer_, len, "< ");
		base::encode(buffer_, len, last);
	}

	using base::encode;

	// Bit error rate
	double ber() const
	{
		return store.ber;
	}

	char lossyByte(char b)
	{
		for(int i = 0; i < 8; i++) {
			double p =
#ifdef STORED_OS_WINDOWS
				(double)::rand() / RAND_MAX;
#else
				// flawfinder: ignore
				drand48();
#endif
			if(p < ber()) {
				// Inject an error.
				b = (char)(b ^ (char)(1 << (rand() % 8)));
				store.injected_errors = store.injected_errors + 1;
			}
		}
		return b;
	}

	virtual size_t mtu() const override
	{
		return store.MTU.as<size_t>();
	}
};

int main()
{
    printf("\nStart synchorizer from ZmqLayer on port 5555.\n");
    synchronizer.map(store);

	stored::SegmentationLayer segmentation;
    stored::SyncConnection const& connection = synchronizer.connect(segmentation);

	stored::ArqLayer arq;
	arq.wrap(segmentation);

	stored::Crc16Layer crc;
	crc.wrap(arq);

	stored::AsciiEscapeLayer escape;
	escape.wrap(crc);

	stored::TerminalLayer terminal;
	terminal.wrap(escape);

    stored::BufferLayer buffer;
	buffer.wrap(terminal);

	LossyChannel lossy;
	lossy.wrap(buffer);

    stored::SyncZmqLayer zmq(nullptr, "tcp://*:5555", true);
    zmq.wrap(lossy);

    stored::Poller poller;
	stored::PollableZmqLayer pollableZmq(zmq, stored::Pollable::PollIn);

	while(true) {
		// 0.1 s timeout, to force keep alive once in a while.
		stored::Poller::Result const& result = poller.poll(100);

		if(result.empty()) {
			switch(errno) {
			case EAGAIN:
			case EINTR:
				break;
			default:
				perror("poll failed");
				return 1;
			}
		}

		zmq.recv();
		synchronizer.process();
	}
	return 0;
}