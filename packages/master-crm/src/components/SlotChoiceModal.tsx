import { FC } from 'react';
import { Button, Div, ModalPage, ModalPageHeader, ModalRoot } from '@vkontakte/vkui';

interface SlotChoiceModalProps {
  active: boolean;
  onClose: () => void;
  onCreateAppointment: () => void;
  onCreateEmptySlot: () => void;
}

export const SlotChoiceModal: FC<SlotChoiceModalProps> = ({
  active,
  onClose,
  onCreateAppointment,
  onCreateEmptySlot,
}) => (
  <ModalRoot activeModal={active ? 'slot-choice' : null} onClose={onClose}>
    <ModalPage
      id="slot-choice"
      onClose={onClose}
      header={<ModalPageHeader>Новый слот</ModalPageHeader>}
    >
      <Div style={{ display: 'grid', gap: 12 }}>
        <Button size="l" mode="primary" stretched onClick={onCreateAppointment}>
          Создать запись
        </Button>
        <Button size="l" mode="secondary" stretched onClick={onCreateEmptySlot}>
          Создать пустой слот
        </Button>
      </Div>
    </ModalPage>
  </ModalRoot>
);

export default SlotChoiceModal;
